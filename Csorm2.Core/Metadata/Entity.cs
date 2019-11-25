using System;
using System.Collections.Generic;

namespace Csorm2.Core.Schema
{
    public class Entity
    {
        public Entity(string entityName, string tableName)
        {
            EntityName = entityName;
            TableName = tableName;
        }

        public string EntityName { get; set; }
        public string TableName { get; set; }
        
        public Type? ClrType { get; set; }
        
        public IReadOnlyDictionary<string, Attribute> Attributes { get; set; }
        
        public Attribute? PrimaryKeyAttribute { get; set; }

        public bool IsShadowEntity => ClrType == null;
        
    }
}