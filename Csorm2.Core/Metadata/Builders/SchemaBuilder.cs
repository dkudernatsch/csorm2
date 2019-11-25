using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Csorm2.Core.Attributes;
using Csorm2.Core.Extensions;

namespace Csorm2.Core.Metadata.Builders
{
    public class SchemaBuilder
    {
        public SchemaBuilder(IEnumerable<Type> specifiedEntityTypes)
        {
            _specifiedEntityTypes = specifiedEntityTypes;
        }

        private IEnumerable<Type> _specifiedEntityTypes;
        public SchemaBuildContext Context { get; } = new SchemaBuildContext();

        private void ScanForEntities()
        {
            var fromTypeBuilders = _specifiedEntityTypes
                .Select(e => new EntityBuilder(Context).FromType(e))
                .ToDictionary(b => b.EntityName);
            Context.EntityBuilders.AddDict(fromTypeBuilders);

            var fromRelationsBuilders = ManyToManyRelationResolver
                .ScanManyToManyRelation(Context).ToList()
                .DistinctBy(b => b.EntityName)
                .ToDictionary(b => b.EntityName);

            Context.EntityBuilders.AddDict(fromRelationsBuilders);
        }

        private void BuildEntities()
        {
            foreach (var (_, entityBuilder) in Context.EntityBuilders)
            {
                entityBuilder.Build();
            }
        }

        private void ScanForAttributes()
        {
            var attributesFromTypes = Context.EntityBuilders.Values
                .Select(eb => (eb, eb.ClrType))
                .Where(tup => tup.ClrType != null)
                .SelectMany(tuple =>
                    AttributesFromTypeResolver.ValidAttributesFromType(tuple.ClrType).Select(prop => (tuple.eb, prop)))
                .Select(tuple =>
                    new AttributeBuilder(Context, Context.Entities[tuple.eb.EntityName]).FromProperty(tuple.prop))
                .GroupBy(ab => ab.Entity.EntityName)
                .ToDictionary(grp => grp.Key, grp => grp.ToDictionary(attr => attr.Name));

            foreach (var (key, value) in attributesFromTypes)
            {
                foreach (var (_, attributeBuilder) in value)
                {
                    Context.AttributeBuilders.GetOrInsert(key,
                        new Dictionary<string, AttributeBuilder>())[attributeBuilder.Name] = attributeBuilder;
                }
                
            }
            
            var attributesFromRelations = Context.AttributeBuilders.ToDictionary(kv => kv.Key, kv => kv.Value)
                .SelectMany(tup => tup.Value.Values.Select(vals => (Entity: tup.Key, Attribute: vals)))
                .Select(tup =>
                    (Entity: tup.Entity
                        , Relation: tup.Attribute.PropertyInfo.GetCustomAttribute(typeof(Relation)) as Relation
                    ))
                .Where(t => t.Relation != null)
                .FilterSelect(tup =>
                    new AttributeBuilder(Context, Context.Entities[tup.Entity]).FromRelation(tup.Relation))
                .DistinctBy(attr => (attr.Name, attr.Entity.EntityName))
                .GroupBy(ab => ab.Entity.EntityName)
                .ToDictionary(grp => grp.Key, grp => grp.ToDictionary(attr => attr.Name));

            foreach (var (key, value) in attributesFromRelations)
            {
                foreach (var (_, attributeBuilder) in value)
                {
                    Context.AttributeBuilders.GetOrInsert(key,
                        new Dictionary<string, AttributeBuilder>())[attributeBuilder.Name] = attributeBuilder;
                }
                
            }
        }

        private void BuildAttributes()
        {
            foreach (var builder in Context.AttributeBuilders.SelectMany(ab => ab.Value.Values))
            {
                builder.Build();
            }
        }


        private void ScanForRelations()
        {
            var relationsFromRelationAttributes = Context.Attributes.Values
                .SelectMany(p => p?.Values)
                .FilterSelect(p => p)
                .Select(attr => (Attribute: attr,
                    Relation: attr.PropertyInfo?.GetCustomAttribute(typeof(Relation)) as Relation))
                .Where(tup => tup.Relation != null)
                .Select(tup => new RelationBuilder(Context, tup.Attribute).FromRelation(tup.Relation))
                .GroupBy(relBuilder => relBuilder.FromEntity.EntityName)
                .ToDictionary(grp => grp.Key, grp => grp.ToDictionary(rel => rel.FromEntityAttribute.PropertyName));

            Context.RelationBuilders.AddDict(relationsFromRelationAttributes);
        }

        private void BuildRelations()
        {
            foreach (var builder in Context.RelationBuilders.SelectMany(ab => ab.Value.Values))
            {
                builder.Build();
            }
        }

        public void Build()
        {
            ScanForEntities();
            BuildEntities();

            ScanForAttributes();
            BuildAttributes();

            ScanForRelations();
            BuildRelations();

            foreach (var (entityName, entity) in Context.Entities)
            {
                entity.Attributes = new ReadOnlyDictionary<string, Attribute>(
                    Context.Attributes[entityName]   
                );
            }
        }
    }
}