using System;
using System.Collections.Generic;

namespace Csorm2.Core.Metadata
{
    /// <summary>
    /// Represents both a C# poco entity and the database table generated by it
    /// </summary>
    public class Entity
    {
        public Entity(string entityName, string tableName)
        {
            EntityName = entityName;
            TableName = tableName;
        }
        /// <summary>
        /// Entity name, generally the name of the c# class
        /// </summary>
        public string EntityName { get; set; }
        /// <summary>
        /// Name of the database table, generally the same as EntityName unless otherwise defined via Table attribute
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// C# type of the entity, this property is null for entities which dont have a c# types, eg manytomany relation tables 
        /// </summary>
        public Type ClrType { get; set; }
        
        /// <summary>
        /// Table Attributes defined for this entity
        /// </summary>
        public IReadOnlyDictionary<string, Attribute> Attributes { get; set; }
        /// <summary>
        /// Primary key attribute
        /// </summary>
        public Attribute PrimaryKeyAttribute { get; set; }
        /// <summary>
        /// Indicates if this entity has a c# representation or not
        /// </summary>
        public bool IsShadowEntity => ClrType == null;
        
    }
}