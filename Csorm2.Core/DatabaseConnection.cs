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
        private readonly DbConnection _inner;
        
        public DatabaseConnection(DbContext ctx, DbConnection conn)
        {
            _ctx = ctx;
            _inner = conn;
            
            if(_inner.State == ConnectionState.Closed) _inner.Open();
        }

        public IEnumerable<TEntity> Select<TEntity>(IQuery<TEntity> query)
        {
            using var cmd = _inner.CreateCommand();
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
            var cache = _ctx.Cache;

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
                    entry.originalEntity[attrName] = value;
                    if (!attr.IsShadowAttribute)
                    {
                        attr.PropertyInfo.SetMethod.Invoke(entry.EntityObject, new[] {value});
                    }
                }
                RegisterLazyLoader((TEntity) entry.EntityObject, entity);
                yield return (TEntity) entry.EntityObject;
            }
        }

        private void RegisterLazyLoader<T>(T entityObj, Entity entity)
        {
            var property = typeof(T).GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(p => p.PropertyType.IsAssignableFrom(typeof(ILazyLoader)));
            if (property != null && property.CanWrite)
            {
                property.GetSetMethod(true).Invoke(entityObj, new object[] {new CachedLazyLoader(entity, _ctx)});
            }
        }
        
    }
}