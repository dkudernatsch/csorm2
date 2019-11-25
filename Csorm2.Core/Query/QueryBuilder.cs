using System.Linq;
using csorm_core.CSORM.Metadata;

namespace csorm_core.CSORM.Query
{


    public class QueryBuilder
    {
        private DbContext _ctx;

        public QueryBuilder(DbContext ctx)
        {
            _ctx = ctx;
        }

        public SelectFromWhereQueryBuilder Select<TEntity>()
        {
            var entity = _ctx.Schema.EntityTypeMap[typeof(TEntity)];
            return Select(entity);
        }
        
        public SelectFromWhereQueryBuilder Select(Entity entity)
        {
            var queryBuilder = new SelectQueryBuilder(_ctx);
            return queryBuilder
                .FromTable(entity.EntityName)
                .Select(entity.Attributes.Values.Where(attr => attr.IsPrimitive).Select(attr => attr.Name));
        }
        
    }
    
    
}