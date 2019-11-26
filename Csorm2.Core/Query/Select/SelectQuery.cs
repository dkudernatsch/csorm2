using System.Collections.Generic;
using System.Linq;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query.Expression;
using DbType = System.Data.DbType;

namespace Csorm2.Core.Query.Select
{
    public class SelectQuery<TEntity>: IQuery<TEntity>
    {
        private readonly IFrom _from;
        public IEnumerable<Attribute> QueryAttributes { get; }
        private readonly IList<WhereSqlFragment> _whereFilters;

        internal SelectQuery(IFrom from, IEnumerable<Attribute> attributes, IList<WhereSqlFragment> whereFilters)
        {
            _from = from;
            QueryAttributes = attributes;
            _whereFilters = whereFilters;
        }

        public string AsSqlString()
        {
            var selectFragment = QueryAttributes
                .Select(attr => $"{attr.DeclaredIn.EntityName}.{attr.DataBaseColumn}")
                .Aggregate("", (s1, s2) => s1 == "" ? $"{s2}" : $"{s1}, {s2}");

            var fromFragment = _from.AsFromFragment();
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