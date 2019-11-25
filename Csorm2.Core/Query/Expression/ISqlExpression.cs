using System.Collections.Generic;
using System.Data;

namespace Csorm2.Core.Query.Expression
{
    public interface ISqlExpression
    {
        string AsSqlString();
        IEnumerable<(DbType, string, object)> GetParameters();

    }
}