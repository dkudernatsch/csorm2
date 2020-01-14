using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace Csorm2.Core.Metadata
{
    public static class DbTypeMap
    {
        private static readonly ReadOnlyDictionary<Type, DbType?> _map = new ReadOnlyDictionary<Type, DbType?>(
            new Dictionary<Type, DbType?>
            {
                {typeof(byte), DbType.Byte},
                {typeof(sbyte), DbType.SByte},
                {typeof(short), DbType.Int16},
                {typeof(ushort), DbType.UInt16},
                {typeof(int), DbType.Int32},
                {typeof(uint), DbType.UInt32},
                {typeof(long), DbType.Int64},
                {typeof(ulong), DbType.UInt64},
                {typeof(float), DbType.Single},
                {typeof(double), DbType.Double},
                {typeof(decimal), DbType.Decimal},
                {typeof(bool), DbType.Boolean},
                {typeof(string), DbType.String},
                {typeof(char), DbType.StringFixedLength},
                {typeof(Guid), DbType.Guid},
                {typeof(DateTime), DbType.DateTime},
                {typeof(DateTimeOffset), DbType.DateTimeOffset},
                {typeof(byte[]), DbType.Binary},
                {typeof(byte?), DbType.Byte},
                {typeof(sbyte?), DbType.SByte},
                {typeof(short?), DbType.Int16},
                {typeof(ushort?), DbType.UInt16},
                {typeof(int?), DbType.Int32},
                {typeof(uint?), DbType.UInt32},
                {typeof(long?), DbType.Int64},
                {typeof(ulong?), DbType.UInt64},
                {typeof(float?), DbType.Single},
                {typeof(double?), DbType.Double},
                {typeof(decimal?), DbType.Decimal},
                {typeof(bool?), DbType.Boolean},
                {typeof(char?), DbType.StringFixedLength},
                {typeof(Guid?), DbType.Guid},
                {typeof(DateTime?), DbType.DateTime},
                {typeof(DateTimeOffset?), DbType.DateTimeOffset}
            }
        );

        private static readonly ReadOnlyDictionary<DbType, string> _ddlMap = new ReadOnlyDictionary<DbType, string>(
            new Dictionary<DbType, string>
            {
                {DbType.Byte, "smallint"},
                {DbType.SByte, "smallint"},
                {DbType.Int16, "smallint"},
                {DbType.UInt16, "smallint"},
                {DbType.Int32, "integer"},
                {DbType.UInt32, "integer"},
                {DbType.Int64, "bigint"},
                {DbType.UInt64, "bigint"},
                {DbType.Single, "real"},
                {DbType.Double, "double"},
                {DbType.Decimal, "numeric"},
                {DbType.Boolean, "boolean"},
                {DbType.String, "text"},
                {DbType.StringFixedLength, "text"},
                {DbType.Guid, "uuid"},
                {DbType.DateTime, "timestamp"},
                {DbType.DateTimeOffset, "timestamptz"},
                {DbType.Binary, "bytea"}
            }
        );


        public static DbType? Map(Type t) => _map.GetValueOrDefault(t, null);
        public static string AsDdl(DbType t) =>
            _ddlMap.GetValueOrDefault(t) ??
            throw new Exception($"Database type not supported: {Enum.GetName(typeof(DbType), t)}");
    }
}