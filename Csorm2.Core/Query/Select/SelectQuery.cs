using System.Collections;
using System.Collections.Generic;
using System.Linq;
using csorm_core.CSORM.Metadata;
using csorm_core.CSORM.Query.Expression;
using DbType = System.Data.DbType;

namespace csorm_core.CSORM.Query
{
    public class SelectQuery: ISqlExpression
    {
        private readonly Entity _entity;
        public IEnumerable<Attribute> QueryAttributes { get; }
        private readonly IList<WhereSqlFragment> _whereFilters;
        public Entity QueryEntity => _entity;

        internal SelectQuery(Entity entity, IEnumerable<Attribute> attributes, IList<WhereSqlFragment> whereFilters)
        {
            _entity = entity;
            QueryAttributes = attributes;
            _whereFilters = whereFilters;
        }

        public string AsSqlString()
        {
            var selectFragment = QueryAttributes
                .Select(attr => $"{attr.DeclaredInEntity.EntityName}.{attr.Name}")
                .Aggregate("", (s1, s2) => s1 == "" ? $"{s2}" : $"{s1}, {s2}");

            var fromFragment = _entity.EntityName;
            var whereFragment = _whereFilters.Select(w => w.AsSqlString()).Aggregate("", (w1, w2) => w1 == "" ? $"{w2}" :$"{w1} AND {w2}");
            
            return $"SELECT {selectFragment} " +
                   $"FROM {fromFragment} " +
                        (whereFragment == "" 
                            ? "" 
                            : $"WHERE {whereFragment} ");
        }

        public IEnumerable<(DbType, string, object)> GetParameters()
            => _whereFilters.SelectMany(w => w.GetParameters());

    }
}