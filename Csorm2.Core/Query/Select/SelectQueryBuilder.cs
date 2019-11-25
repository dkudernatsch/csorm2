using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using csorm_core.CSORM.Metadata;
using csorm_core.CSORM.Query.Expression;

namespace csorm_core.CSORM.Query
{
    public class SelectQueryBuilder
    {
        private readonly DbContext _ctx;

        public SelectQueryBuilder(DbContext ctx)
        {
            _ctx = ctx;
        }

        public SelectFromQueryBuilder FromTable(string table)
        {
            return new SelectFromQueryBuilder(_ctx, table);
        }
        
        
    }

    public class SelectFromQueryBuilder
    {
        private readonly string _tableName;
        private readonly DbContext _ctx;

        public SelectFromQueryBuilder(DbContext ctx, string tableName)
        {
            _tableName = tableName;
            _ctx = ctx;
        }

        public SelectFromWhereQueryBuilder Select(IEnumerable<string> fields)
        {
            return new SelectFromWhereQueryBuilder(_ctx, _tableName, fields);
        }
        
    }

    public class SelectFromWhereQueryBuilder
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

        public SelectFromWhereQueryBuilder Where(WhereSqlFragment filter)
        {
            _wheres.Add(filter);
            return this;
        }

        public SelectQuery Build()
        {
            var entity = _ctx.Schema.EntityNameMap[_tableName];
            return new SelectQuery(entity, entity.Attributes.Values.Where(attr => attr.IsPrimitive), _wheres);
        }
    }
}