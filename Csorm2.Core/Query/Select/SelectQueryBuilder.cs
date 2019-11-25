using System.Collections.Generic;
using System.Linq;
using Csorm2.Core.Query.Expression;

namespace Csorm2.Core.Query.Select
{
    public class SelectQueryBuilder
    {
        private readonly DbContext _ctx;

        public SelectQueryBuilder(DbContext ctx)
        {
            _ctx = ctx;
        }

        public SelectFromQueryBuilder<TEntity> FromTable<TEntity>(string table)
        {
            return new SelectFromQueryBuilder<TEntity>(_ctx, table);
        }
        
        
    }

    public class SelectFromQueryBuilder<TEntity>
    {
        private readonly string _tableName;
        private readonly DbContext _ctx;

        public SelectFromQueryBuilder(DbContext ctx, string tableName)
        {
            _tableName = tableName;
            _ctx = ctx;
        }

        public SelectFromWhereQueryBuilder<TEntity> Select(IEnumerable<string> fields)
        {
            return new SelectFromWhereQueryBuilder<TEntity>(_ctx, _tableName, fields);
        }
        
    }

    public class SelectFromWhereQueryBuilder<TEntity>
    {
        
        private readonly string _tableName;
        private IEnumerable<string> _fields;
        private readonly IList<WhereSqlFragment> _wheres = new List<WhereSqlFragment>();
        private readonly DbContext _ctx;

        public SelectFromWhereQueryBuilder(DbContext ctx, string tableName, IEnumerable<string> fields)
        {
            _tableName = tableName;
            _fields = fields;
            _ctx = ctx;
        }

        public SelectFromWhereQueryBuilder<TEntity> Where(WhereSqlFragment filter)
        {
            _wheres.Add(filter);
            return this;
        }

        public SelectQuery<TEntity> Build()
        {
            var entity = _ctx.Schema.EntityNameMap[_tableName];
            return new SelectQuery<TEntity>(entity, entity.Attributes.Values.Where(attr => !attr.IsEntityType), _wheres);
        }
    }
}