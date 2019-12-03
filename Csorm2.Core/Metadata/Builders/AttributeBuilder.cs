using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Csorm2.Core.Attributes;
using Csorm2.Core.Extensions;

namespace Csorm2.Core.Metadata.Builders
{
    public class AttributeBuilder
    {
        private readonly SchemaBuildContext _context;
        public Entity Entity { get; }

        private Type _clrType;
        public string Name { get; private set; }
        private string _databaseName;
        public PropertyInfo PropertyInfo { get; private set; }
        private bool _isAutoInc = false;
        
        public AttributeBuilder(SchemaBuildContext context, Entity entity)
        {
            _context = context;
            Entity = entity;
        }

        public AttributeBuilder FromRelation(Relation r)
        {
            switch (r)
            {
                case Attributes.OneToOne relation:
                {
                    return new AttributeBuilder(_context, Entity)
                    {
                        _clrType = null, Name = relation.ThisKey, _databaseName = relation.ThisKey
                    };
                }
                case Attributes.ManyToOne relation:
                {
                    var otherType = _context.AttributeBuilders
                        .GetValueOrDefault(_context.Entities.First(e => e.Value.ClrType == relation.OtherEntity).Value.EntityName)?
                        .GetValueOrDefault(relation.OtherKey)?._clrType;
                    
                    return new AttributeBuilder(_context, Entity)
                    {
                        _clrType = otherType, Name = relation.ThisKey, _databaseName = relation.ThisKey
                    };
                }
                case Attributes.ManyToMany relation:
                {
                    var tableEntity = relation.RelationTableName;
                    const string primaryKeyAttr = "Id";
                    var toThisEntity = relation.OtherEntityOtherKey;
                    var toOtherEntity = relation.OtherKey;
                    
                    _context.AttributeBuilders
                        .GetOrInsert(tableEntity, new Dictionary<string, AttributeBuilder>())[toThisEntity] = new AttributeBuilder(_context, _context.Entities[tableEntity])
                    {
                        _clrType = null,
                        _databaseName = toThisEntity,
                        Name = toThisEntity,
                    };
                    
                    _context.AttributeBuilders
                        .GetOrInsert(tableEntity, new Dictionary<string, AttributeBuilder>())[toOtherEntity] = new AttributeBuilder(_context, _context.Entities[tableEntity])
                    {
                        _clrType = null,
                        _databaseName = toOtherEntity,
                        Name = toOtherEntity,
                    };
                    
                    _context.AttributeBuilders
                        .GetOrInsert(tableEntity, new Dictionary<string, AttributeBuilder>())[primaryKeyAttr] = new AttributeBuilder(_context, _context.Entities[tableEntity])
                    {
                        _clrType = null,
                        _databaseName = primaryKeyAttr,
                        Name = primaryKeyAttr,
                        _isAutoInc = true,
                    };
                    
                    return null;
                }
                case Attributes.OneToMany relation:
                {
                    var otherType = _context.AttributeBuilders
                        .GetValueOrDefault(_context.Entities.First(e => e.Value.ClrType == Entity.ClrType).Value.EntityName)?
                        .GetValueOrDefault(relation.ThisKey)?._clrType;
                    
                    return new AttributeBuilder(_context, _context.Entities.First(e => e.Value.ClrType ==relation.OtherEntity).Value)
                    {
                        _clrType = otherType, Name = relation.OtherKey, _databaseName = relation.OtherKey
                    };
                }
            }
            throw new ArgumentException("relation does not have a supported type", nameof(r));
        }
        
        public AttributeBuilder FromProperty(PropertyInfo prop)
        {
            var col = prop.GetCustomAttribute(typeof(Column)) as Column;
            var colName = col?.ColumnName ?? prop.Name;
            return new AttributeBuilder(_context, Entity)
            {
                _clrType = prop.PropertyType,
                Name = prop.Name,
                _databaseName = colName,
                PropertyInfo = prop,
                _isAutoInc = prop.GetCustomAttribute<AutoIncrement>() != null,
            };
        }

        public void Build()
        {
            DbType? dbType = null;
            if(_clrType != null) dbType = DbTypeMap.Map(_clrType);
            
            var attr = new Attribute(_clrType, Name, _databaseName, PropertyInfo, dbType, _isAutoInc)
            {
                DeclaredIn = Entity
            };
            
            _context.Attributes.GetOrInsert(Entity.EntityName, new Dictionary<string, Attribute>())[Name] = attr;
            if (IsPrimaryKey())
            {
                attr.DeclaredIn.PrimaryKeyAttribute = attr;
            }
        }

        private bool IsPrimaryKey()
        {
            if (Entity.IsShadowEntity)
            {
                return Name == "Id";
            }
            
            return PropertyInfo?.GetCustomAttribute(typeof(PrimaryKey))
                is PrimaryKey;
        }
        
    }
}