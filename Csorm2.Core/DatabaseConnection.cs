using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Csorm2.Core.Cache;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query;

namespace Csorm2.Core
{
    public class DatabaseConnection
    {
        private readonly DbContext _ctx;
        private readonly Func<IDbConnection> _connProvider;
        
        public DatabaseConnection(DbContext ctx, Func<IDbConnection> connProvider)
        {
            _ctx = ctx;
            _connProvider = connProvider;
        }

        public IEnumerable<TEntity> Select<TEntity>(IQuery<TEntity> query)
        {
            using var conn = _connProvider.Invoke();
            conn.Open();
            var cache = _ctx.Cache;
            using var cmd = conn.CreateCommand();
            cmd.CommandText = query.AsSqlString();
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
                .FirstOrDefault(p => p.PropertyType.IsAssignableFrom(typeof(ILazyLoader)));
            if (property != null && property.CanWrite)
            {
                property.GetSetMethod(true).Invoke(entityObj, new object[] {new CachedLazyLoader(entity, _ctx, entry)});
            }
        }

        public void Insert<TEntity>(IStatement<TEntity> stmt)
        {
            using var conn = _connProvider.Invoke();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = stmt.AsSqlString();
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
                RegisterLazyLoader(entry.EntityObject, entity, entry);
            }
        }

        public void Update<TEntity>(IStatement<TEntity> stmt)
        {
            using var conn = _connProvider.Invoke();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = stmt.AsSqlString();
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
    }
}