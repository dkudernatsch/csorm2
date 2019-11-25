using System;
using System.Collections.Generic;
using System.Data;
using Csorm2.Core.Metadata;
using Attribute = Csorm2.Core.Metadata.Attribute;

namespace Csorm2.Core.Query.Expression
{
    public class Value: ISqlExpression
    {
        private Value(string name, object value, DbType dbType)
        {
            _name = name;
            _value = value;
            _dbType = dbType;
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

        public static Value FromAttr(object val, Attribute attr)
        {
            return new Value(attr.Name, val, attr.DatabaseType.GetValueOrDefault());
        }
        
        public static Value Val<T>(T i, string name)
        {
            return new Value(name, i, DbTypeMap.Map(typeof(T)).GetValueOrDefault(DbType.String));
        }
        
        public static Value Integer(int i, string name){ return new Value(name, i, DbType.Int32);}
        public static Value Boolean(bool i, string name){ return new Value(name, i, DbType.Boolean);}
        public static Value Float(float i, string name){ return new Value(name, i, DbType.Double);}
        public static Value String(string i, string name){ return new Value(name, i, DbType.String);}
        public static Value Date(DateTime i, string name){ return new Value(name, i, DbType.DateTime);}
    }
}