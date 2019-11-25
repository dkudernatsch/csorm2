using System;
using System.Collections.Generic;
using System.Linq;
using Csorm2.Core.Attributes;
using Csorm2.Core.Extensions;

namespace Csorm2.Core.Schema.Builders
{
    public class RelationBuilder
    {

        private Attributes.Relation _relation;
        
        // entity in which this relation is saved
        public Entity FromEntity { get; set; }

        // entity which this relation references
        public Entity ToEntity { get; set; }

        // foreign/primary key attribute on this entity
        public  Attribute FromKeyAttribute { get; set; }

        // Entity attribute that this relation forms on this Entity
        public Attribute? FromEntityAttribute { get; set; }

        // foreign/primary key attribute on other entity
        public Attribute ToKeyAttribute { get; set; }

        // Entity attribute that this relation forms on other Entity
        public Attribute? ToEntityAttribute { get; }
        
        // 'shadow entity' necessary for this relation to exists
        public Entity BetweenEntity { get; set; }

        // key attribute that this entity references on the relation table
        public Attribute ReferencedFromAttribute { get; set; }

        // key attribute that the referenced entity references on the relation table
        public Attribute ReferencedToAttribute { get; set; }
        
        private Attribute _attr;
        private SchemaBuildContext _context;

        public RelationBuilder(SchemaBuildContext context, Attribute attribute)
        {
            _context = context;
            _attr = attribute;
        }


        public RelationBuilder FromRelation(Relation relation)
        {

            var relBuilder = new RelationBuilder(_context, _attr) {_relation = relation};
            
            
            
            if (relation is Attributes.ManyToMany r){
                
                relBuilder.FromEntity = relBuilder.FromEntity = _attr.DeclaredIn;
            
                relBuilder.ToEntity = _context.Entities.First(e => e.Value.ClrType == relation.OtherEntity).Value;
                relBuilder.FromEntityAttribute = _attr;
                    
               
                
                relBuilder.BetweenEntity = _context.Entities[r.RelationTableName];
                relBuilder.FromEntity = _attr.DeclaredIn;
                relBuilder.ToEntity = _context.Entities.First(e => e.Value.ClrType == r.OtherEntity).Value;
                
                relBuilder.ToKeyAttribute = _context.Attributes[relBuilder.BetweenEntity.EntityName][relation.OtherKey];
                relBuilder.FromKeyAttribute = _context.Attributes[relBuilder.FromEntity.EntityName][relation.ThisKey];
                
                relBuilder.FromEntityAttribute = _attr;
                relBuilder.FromKeyAttribute = _context.Attributes[_attr.DeclaredIn.EntityName][r.ThisKey];

                relBuilder.ToKeyAttribute =
                    _context.Attributes[relBuilder.ToEntity.EntityName][r.OtherEntityThisKey];

                relBuilder.ReferencedFromAttribute =
                    _context.Attributes[relBuilder.BetweenEntity.EntityName][r.OtherKey];
                relBuilder.ReferencedToAttribute =
                    _context.Attributes[relBuilder.BetweenEntity.EntityName][r.OtherEntityOtherKey];
            }
            else if (relation is Attributes.OneToMany)
            {
                relBuilder.FromEntity = relBuilder.FromEntity = _attr.DeclaredIn;
            
                relBuilder.ToEntity = _context.Entities.First(e => e.Value.ClrType == relation.OtherEntity).Value;
                relBuilder.FromEntityAttribute = _attr;
                    
                relBuilder.ToKeyAttribute = _context.Attributes[relBuilder.ToEntity.EntityName][relation.OtherKey];
                relBuilder.FromKeyAttribute = _context.Attributes[relBuilder.FromEntity.EntityName][relation.ThisKey];
            }
            else
            {
                relBuilder.FromEntity = relBuilder.FromEntity = _attr.DeclaredIn;
            
                relBuilder.ToEntity = _context.Entities.First(e => e.Value.ClrType == relation.OtherEntity).Value;
                relBuilder.FromEntityAttribute = _attr;
                    
                relBuilder.ToKeyAttribute = _context.Attributes[relBuilder.ToEntity.EntityName][relation.OtherKey];
                relBuilder.FromKeyAttribute = _context.Attributes[relBuilder.FromEntity.EntityName][relation.ThisKey];
            }
            return relBuilder;
        }

        public void Build()
        {
            var relation = _relation switch
            {
                Attributes.OneToOne a => new OneToOne(FromEntity, ToEntity, FromKeyAttribute, FromEntityAttribute,
                    ToKeyAttribute, ToEntityAttribute) as IRelation,
                Attributes.ManyToOne b => new ManyToOne(FromEntity, ToEntity, FromKeyAttribute, FromEntityAttribute,
                    ToKeyAttribute, ToEntityAttribute),
                Attributes.OneToMany c => new OneToMany(FromEntity, ToEntity, FromKeyAttribute, FromEntityAttribute,
                    ToKeyAttribute, ToEntityAttribute),
                Attributes.ManyToMany c => new ManyToMany(FromEntity, ToEntity, FromKeyAttribute, FromEntityAttribute,
                    ToKeyAttribute, ToEntityAttribute, BetweenEntity, ReferencedFromAttribute, ReferencedToAttribute),
                _ => throw new ArgumentException(nameof(_relation))
            };

            relation.FromEntityAttribute.Relation = relation;
            _context.Relations.GetOrInsert(relation.FromEntity.EntityName, new Dictionary<string, IRelation>())[
                relation.FromEntityAttribute.PropertyName] = relation;
        }
    }
}