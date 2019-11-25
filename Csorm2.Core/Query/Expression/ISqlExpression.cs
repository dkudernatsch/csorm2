using System.Collections.Generic;
using System.Data;

namespace csorm_core.CSORM.Query.Expression
{
    public interface ISqlExpression
    {
        string AsSqlString();
        IEnumerable<(DbType, string, object)> GetParameters();

    }
}