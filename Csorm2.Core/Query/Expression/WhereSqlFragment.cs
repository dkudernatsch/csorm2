using System.Collections.Generic;
using System.Data;

namespace csorm_core.CSORM.Query.Expression
{
    public class WhereSqlFragment: ISqlExpression
    {

        private ISqlExpression _inner;

        public WhereSqlFragment(ISqlExpression inner)
        {
            _inner = inner;
        }

        public string AsSqlString()
        {
            return _inner.AsSqlString();
        }

        public IEnumerable<(DbType, string, object)> GetParameters()
        {
            return _inner.GetParameters();
        }
    }
}