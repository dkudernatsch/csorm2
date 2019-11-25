using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Csorm2.Core.Attributes;

namespace Csorm2.Core.Metadata.Builders
{
    public class EntityBuilder
    {

        private SchemaBuildContext _context;

        public Type ClrType { get; private set; }
        public string EntityName { get; private set; }
        private string _tableName;
        
        private IEnumerable<PropertyInfo> _attributesFromType = null;
        
        public EntityBuilder(SchemaBuildContext context)
        {
            _context = context;
        }

        public EntityBuilder FromType(Type t)
        {
            var builder = new EntityBuilder(_context) {ClrType = t, EntityName = t.Name};
            
            var tAttr = t.GetCustomAttribute(typeof(Table)) as Table;
            builder._tableName = tAttr?.TableName ?? builder.EntityName;
            
            builder._attributesFromType = t.GetProperties().Where(p => p.CanRead && p.CanWrite);
            
            return builder;
        }

        public EntityBuilder FromManyToManyRelation(Attributes.ManyToMany rel)
        {
            var builder = new EntityBuilder(_context)
            {
                ClrType = null, EntityName = rel.RelationTableName, _tableName = rel.RelationTableName,
            };
            return builder;
        }

        public void Build()
        {
            var entity = new Entity(EntityName, _tableName)
            {
                ClrType = ClrType,
            };
            _context.Entities[entity.EntityName] = entity;
        }
    }
}