using System;
using System.Linq;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query.Insert;
using Csorm2.Core.Query.Select;

namespace Csorm2.Core.Query
{
    public class QueryBuilder
    {
        private DbContext _ctx;

        public QueryBuilder(DbContext ctx)
        {
            _ctx = ctx;
        }
        
        public SelectFromWhereQueryBuilder<TEntity> Select<TEntity>()
        {
            var entity = _ctx.Schema.EntityTypeMap[typeof(TEntity)];
            return Select<TEntity>(entity);
        }
        
        public SelectFromWhereQueryBuilder<TEntity> Select<TEntity>(Entity entity)
        {
            var queryBuilder = new SelectQueryBuilder(_ctx);
            return queryBuilder
                .FromTable<TEntity>(entity.EntityName)
                .Select(entity.Attributes.Values.Where(attr => !attr.IsEntityType).Select(attr => attr.DataBaseColumn));
        }

        public InsertStatement<TEntity> Insert<TEntity>(TEntity value)
        {
            return new InsertQueryBuilder(_ctx)
                .Insert<TEntity>()
                .Value(value);
        }
        
    }
    
    
}