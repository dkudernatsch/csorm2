namespace Csorm2.Core.Schema
{
    public interface IRelation
    {
        // entity in which this relation is saved
        Entity FromEntity { get; }

        // entity which this relation references
        Entity ToEntity { get; }

        // foreign/primary key attribute on this entity
        Attribute FromKeyAttribute { get; }

        // Entity attribute that this relation forms on this Entity
        Attribute? FromEntityAttribute { get; }

        // foreign/primary key attribute on other entity
        Attribute ToKeyAttribute { get; }

        // Entity attribute that this relation forms on other Entity
        Attribute? ToEntityAttribute { get; }
    }

    public class AbstractRelation : IRelation
    {
        public AbstractRelation(Entity fromEntity, Entity toEntity, Attribute fromKeyAttribute,
            Attribute fromEntityAttribute, Attribute toKeyAttribute, Attribute toEntityAttribute)
        {
            FromEntity = fromEntity;
            ToEntity = toEntity;
            FromKeyAttribute = fromKeyAttribute;
            FromEntityAttribute = fromEntityAttribute;
            ToKeyAttribute = toKeyAttribute;
            ToEntityAttribute = toEntityAttribute;
        }

        // entity in which this relation is saved
        public Entity FromEntity { get; set; }

        // entity which this relation references
        public Entity ToEntity { get; set; }

        // foreign/primary key attribute on this entity
        public Attribute FromKeyAttribute { get; set; }

        // Entity attribute that this relation forms on this Entity
        public Attribute? FromEntityAttribute { get; set; }

        // foreign/primary key attribute on other entity
        public Attribute ToKeyAttribute { get; set; }

        // Entity attribute that this relation forms on other Entity
        public Attribute? ToEntityAttribute { get; set; }
    }

    public class OneToOne : AbstractRelation
    {
        public OneToOne(Entity fromEntity, Entity toEntity, Attribute fromKeyAttribute, Attribute fromEntityAttribute,
            Attribute toKeyAttribute, Attribute toEntityAttribute) : base(fromEntity, toEntity, fromKeyAttribute,
            fromEntityAttribute, toKeyAttribute, toEntityAttribute)
        {
        }
    }

    public class ManyToOne : AbstractRelation
    {
        public ManyToOne(Entity fromEntity, Entity toEntity, Attribute fromKeyAttribute, Attribute fromEntityAttribute,
            Attribute toKeyAttribute, Attribute toEntityAttribute) : base(fromEntity, toEntity, fromKeyAttribute,
            fromEntityAttribute, toKeyAttribute, toEntityAttribute)
        {
        }
    }

    public class OneToMany : AbstractRelation
    {
        public OneToMany(Entity fromEntity, Entity toEntity, Attribute fromKeyAttribute, Attribute fromEntityAttribute,
            Attribute toKeyAttribute, Attribute toEntityAttribute) : base(fromEntity, toEntity, fromKeyAttribute,
            fromEntityAttribute, toKeyAttribute, toEntityAttribute)
        {
        }
    }


    public class ManyToMany : AbstractRelation
    {
        public ManyToMany(Entity fromEntity, Entity toEntity, Attribute fromKeyAttribute,
            Attribute fromEntityAttribute, Attribute toKeyAttribute, Attribute toEntityAttribute, Entity betweenEntity,
            Attribute referencedFromAttribute, Attribute referencedToAttribute) : base(fromEntity, toEntity,
            fromKeyAttribute, fromEntityAttribute, toKeyAttribute, toEntityAttribute)
        {
            BetweenEntity = betweenEntity;
            ReferencedFromAttribute = referencedFromAttribute;
            ReferencedToAttribute = referencedToAttribute;
        }

        // 'shadow entity' necessary for this relation to exists
        public Entity BetweenEntity { get; set; }

        // key attribute that this entity references on the relation table
        public Attribute ReferencedFromAttribute { get; set; }

        // key attribute that the referenced entity references on the relation table
        public Attribute ReferencedToAttribute { get; set; }
    }
}