using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Csorm2.Core.Cache;
using Csorm2.Core.Extensions;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query;

namespace Csorm2.Core
{
    public class DatabaseConnection: IDisposable
    {
        private readonly DbContext _ctx;
        private readonly IDbConnection _connection;
        private IDbTransaction _transaction = null;

        public DatabaseConnection(DbContext ctx, IDbConnection connection)
        {
            _ctx = ctx;
            _connection = connection;
        }

        public IEnumerable<TEntity> Select<TEntity>(IQuery<TEntity> query)
        {
            var conn = _connection;
            
            if(conn.State != ConnectionState.Open) conn.Open();
            
            var cache = _ctx.Cache;
            using var cmd = conn.CreateCommand();
            cmd.CommandText = query.AsSqlString();
            cmd.Transaction = _transaction;

            foreach (var (type, id, value) in query.GetParameters())
            {
                var param = cmd.CreateParameter();
                param.Value = value;
                param.DbType = type;
                param.ParameterName = id;
                cmd.Parameters.Add(param);
            }

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var entity = _ctx.Schema.EntityTypeMap[typeof(TEntity)];
                var pkAttr = entity.PrimaryKeyAttribute;

                var pk = reader[pkAttr.DataBaseColumn];

                var entry = cache.GetOrInsert(entity, pk, ObjectProvider.Construct(entity.ClrType));

                foreach (var (attrName, attr) in entity.Attributes)
                {
                    if (attr.IsEntityType) continue;

                    var value = reader[attr.DataBaseColumn];
                    entry.OriginalEntity[attrName] = value;
                    if (!attr.IsShadowAttribute)
                    {
                        attr.PropertyInfo.SetMethod.Invoke(entry.EntityObject, new[] {value});
                    }
                }
                RegisterLazyLoader((TEntity) entry.EntityObject, entity, entry);
                yield return (TEntity) entry.EntityObject;
            }
        }

        private void RegisterLazyLoader<T>(T entityObj, Entity entity, CacheEntry entry)
        {
            var property = typeof(T).GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(p => p.PropertyType.IsAssignableFrom(typeof(LazyLoader)));
            if (property != null && property.CanRead)
            {
                var loader = property.GetGetMethod(true).Invoke(entityObj, new object[] {});
                if (loader is LazyLoader lazy)
                {
                    lazy.Internal = new CachedLazyLoader(entity, _ctx, entry);
                }
            }
        }

        public void Insert<TEntity>(IStatement<TEntity> stmt)
        {
            var conn = _connection;
            if(conn.State != ConnectionState.Open) conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = stmt.AsSqlString();
            cmd.Transaction = _transaction;

            foreach (var (type, id, value) in stmt.GetParameters())
            {
                var param = cmd.CreateParameter();
                param.Value = value ?? DBNull.Value;
                param.DbType = type;
                param.ParameterName = id;
                cmd.Parameters.Add(param);
            }
            using var reader = cmd.ExecuteReader();
            using var returnValuePos = stmt.ReturnValuePositions.GetEnumerator();
            var objectList = new List<TEntity>();
            while (reader.Read() && returnValuePos.MoveNext())
            {
                var valuesForEntity = returnValuePos.Current;
                if(valuesForEntity == null) break;

                foreach (var (attr, obj) in valuesForEntity)
                {
                    var val = reader[attr.DataBaseColumn];
                    attr.PropertyInfo.SetMethod.Invoke(obj, new[] {val});
                    objectList.Add(obj);
                }
            }

            foreach (var obj in objectList)
            {
                var entity = stmt.Entity;
                var pk = entity.PrimaryKeyAttribute.PropertyInfo.GetMethod.Invoke(obj, new object[0]);
                var entry = _ctx.Cache.GetOrInsert(entity, pk, obj);
                
                foreach (var (attrName, attr) in entity.Attributes)
                {
                    if (attr.IsEntityType)
                    {
                        if (attr.Relation is OneToMany)
                        {
                            var otherEntity = attr.Relation.ToEntity;
                            var otherObj = attr.PropertyInfo.GetMethod.Invoke(obj, new object[0]);
                            if(otherObj == null) continue;
                            var otherKey = otherEntity.PrimaryKeyAttribute.PropertyInfo.GetMethod.Invoke(otherObj, new object[0]);
                            if (otherKey == null) throw new Exception("Trying to insert entity with untracked related entity");
                            entry.OriginalEntity[attr.Relation.FromKeyAttribute.Name] = otherKey;
                        } 
                        else
                        {
                            
                        }
                    }
                    else if(!attr.IsShadowAttribute)
                    {
                        var value = obj == null ? null : attr.PropertyInfo.GetMethod.Invoke(obj, new object[0]);
                        entry.OriginalEntity[attrName] = value;
                    }
                    else
                    {
                        entry.OriginalEntity.GetOrInsert(attrName, null);
                    }
                }
                RegisterLazyLoader(entry.EntityObject, entity, entry);
            }
        }

        public void Update<TEntity>(IStatement<TEntity> stmt)
        {
            var conn = _connection;
            if(conn.State != ConnectionState.Open) conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = stmt.AsSqlString();
            cmd.Transaction = _transaction;
            foreach (var (type, id, value) in stmt.GetParameters())
            {
                var param = cmd.CreateParameter();
                param.Value = value;
                param.DbType = type;
                param.ParameterName = id;
                cmd.Parameters.Add(param);
            }
            using var reader = cmd.ExecuteReader();
            using var returnValuePos = stmt.ReturnValuePositions.GetEnumerator();
            while (reader.Read() && returnValuePos.MoveNext())
            {
                var returns = returnValuePos.Current;
                if(returns == null) break;

                var entity = stmt.Entity;
                var newPk = reader[entity.PrimaryKeyAttribute.DataBaseColumn];
                var cacheEntry = _ctx.Cache.ObjectPool[entity].GetValueOrDefault(newPk)
                                 ?? throw new Exception("Entity to update not found in local cache; Only managed entities can be updated");

                foreach (var (attr, obj) in returns)
                {
                    if (attr.IsEntityType) continue;
                    var value = reader[attr.DataBaseColumn];
                    cacheEntry.OriginalEntity[attr.Name] = value;
                    if (!attr.IsShadowAttribute)
                    {
                        attr.PropertyInfo.SetMethod.Invoke(cacheEntry.EntityObject, new[] {value});
                    }
                }
            }
        }

        public void Delete<TEntity>(IStatement<TEntity> stmt)
        {
            var conn = _connection;
            if(conn.State != ConnectionState.Open) conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = stmt.AsSqlString();
            cmd.Transaction = _transaction;
            foreach (var (type, id, value) in stmt.GetParameters())
            {
                var param = cmd.CreateParameter();
                param.Value = value;
                param.DbType = type;
                param.ParameterName = id;
                cmd.Parameters.Add(param);
            }
            var affectedRows = cmd.ExecuteNonQuery();
             
        }
        
        public void ExecuteDdl(string ddlString)
        {
            var conn = _connection;
            if(conn.State != ConnectionState.Open) conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = ddlString;
            cmd.Transaction = _transaction;
            try
            {
                cmd.ExecuteNonQuery();
                //transaction.Commit();
                Console.WriteLine("Transaction successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                Console.WriteLine("  Message: {0}", ex.Message);
                //transaction.Rollback();
            }
        }

        
        public void BeginTransaction()
        {
            if(_connection.State != ConnectionState.Open) _connection.Open();
            _transaction = this._connection.BeginTransaction();
        }

        public void Commit()
        {
            this._transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }
        
        public void Dispose()
        {
            _transaction?.Rollback();
            _transaction?.Dispose();
            _connection.Dispose();
        }
    }
}
