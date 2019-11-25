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


        public static DbType? Map(Type t)
        {
            return _map.GetValueOrDefault(t, null);
        }
    }
}