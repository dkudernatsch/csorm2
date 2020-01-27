using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Csorm2.Core.Cache;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query;
using Csorm2.Core.Query.Insert;
using Csorm2.Core.Query.Update;

namespace Csorm2.Core
{
    /// <summary>
    /// Provides direct communication to the database
    /// Abstraction over ado net IDbConnection which uses Queries and Statement of this library
    /// </summary>
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
        
        /// <summary>
        /// Executes the given SelectQuery and returns the result set transformed back into entity objects.
        /// Adds each returned object to the internal cache as managed entity
        /// </summary>
        /// <param name="query"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public IEnumerable<TEntity> Select<TEntity>(IQuery<TEntity> query)
        {
            var conn = _connection;
            
            if(conn.State != ConnectionState.Open) conn.Open();
            
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

            using var reader = new CachedDataReader(
                _ctx.Cache,
                _ctx.Schema.EntityTypeMap[typeof(TEntity)],
                cmd.ExecuteReader(), 
                _ctx
                );

            foreach (var cacheEntry in reader.ReadAll())
            {
                RegisterLazyLoader((TEntity) cacheEntry.Object, cacheEntry.Entity, cacheEntry);
                yield return (TEntity) cacheEntry.Object;
            }
        }

        private void RegisterLazyLoader(object entityObj, Entity entity, CacheEntry entry)
        {
            var property = entity.ClrType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
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
        /// <summary>
        /// Executes the given InsertStatement and maps returned values back into entities
        /// objects created like this are add into the cache as managed entities
        /// </summary>
        /// <param name="stmt"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <exception cref="Exception"></exception>
        public void Insert<TEntity>(InsertExpression<TEntity> stmt)
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
            
            using var reader = new CachedDataReader(
                _ctx.Cache,
                stmt.Entity,
                cmd.ExecuteReader(),
                _ctx
            );

            var entries = reader
                .ReadAllInto(stmt.InsertedObjects.Cast<object>())
                .ToList();
            
            foreach (var cacheEntry in entries)
            {
                RegisterLazyLoader(cacheEntry.Object, cacheEntry.Entity, cacheEntry);
            }
        }

        public void InsertStatement<T>(InsertExpression<T> stmt)
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
            using var result = cmd.ExecuteReader();
        }
        
        public void Update<TEntity>(UpdateStatement<TEntity> stmt)
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
            
            using var reader = new CachedDataReader(
                _ctx.Cache,
                stmt.Entity,
                cmd.ExecuteReader(),
                _ctx
            );
            
            foreach (var _ in reader.ReadAll().ToList())
            {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                Console.WriteLine("  Message: {0}", ex.Message);
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
