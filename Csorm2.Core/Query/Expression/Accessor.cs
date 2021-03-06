using System;
using System.Collections.Generic;
using System.Data;

namespace Csorm2.Core.Query.Expression
{
    public class Accessor: ISqlExpression
    {
        public string TableName { get; set; }
        public string PropertyName { get; set; }

        public string AsSqlString()
        {
            return TableName + "." + PropertyName;
        }

        public IEnumerable<(DbType, string, object)> GetParameters()
        {
            return Array.Empty<(DbType, string, object)>();
        }
    }
}