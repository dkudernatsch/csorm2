using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualBasic.CompilerServices;

namespace csorm_core.CSORM.Query.Expression
{
    public class Value: ISqlExpression
    {
        private Value(string name, object value, DbType dbType)
        {
            _name = name;
            _value = value;
        }

        private string _name;
        private object _value;
        private DbType _dbType;

        public string AsSqlString()
        {
            return $"@{_name}";
        }

        public IEnumerable<(DbType, string, object)> GetParameters()
        {
            return new List<(DbType, string, object)>{(_dbType, _name, _value)};
        }
        
        public static Value Integer(int i, string name){ return new Value(name, i, DbType.Int32);}
        public static Value Boolean(bool i, string name){ return new Value(name, i, DbType.Boolean);}
        public static Value Float(float i, string name){ return new Value(name, i, DbType.Double);}
        public static Value String(string i, string name){ return new Value(name, i, DbType.String);}
        public static Value Date(DateTime i, string name){ return new Value(name, i, DbType.DateTime);}
    }
}