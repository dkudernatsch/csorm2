using System;
using System.Data;
using System.Reflection;

namespace Csorm2.Core.Metadata
{
    public class Attribute
    {
        public Attribute(
            Type clrType, 
            string name,
            string dataBaseColumn, 
            PropertyInfo propertyInfo,
            DbType? databaseType, 
            bool isAutoInc)
        {
            ClrType = clrType;
            Name = name;
            DataBaseColumn = dataBaseColumn;
            PropertyInfo = propertyInfo;
            DatabaseType = databaseType;
            IsAutoInc = isAutoInc;
        }
        
        public Entity DeclaredIn { get; set; }
        
        public Type ClrType { get; }

        public string Name { get; set; }
        
        /// <summary>
        /// C# Property name
        /// </summary>
        public string PropertyName => PropertyInfo?.Name;
        
        /// <summary>
        /// Database Column field
        /// </summary>
        public string DataBaseColumn { get; }
        
        // if we dont have a backing field the attribute is a shadow property and we
        // cant simply look into the type to get its value
        public PropertyInfo PropertyInfo { get; } = null;
        public bool IsShadowAttribute => PropertyInfo == null;
        
        // if we cant find a matching dbType only other entities are valid
        public DbType? DatabaseType { get; }
        public bool IsEntityType => DatabaseType == null;

        public IRelation Relation { get; set; } = null;

        public bool IsAutoInc { get; set; } = false;
    }
}