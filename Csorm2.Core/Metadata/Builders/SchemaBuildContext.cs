using System.Collections.Generic;

namespace Csorm2.Core.Metadata.Builders
{
    public class SchemaBuildContext
    {
        public Dictionary<string, EntityBuilder> EntityBuilders { get; } 
            = new Dictionary<string, EntityBuilder>();
        
        public Dictionary<string, Entity> Entities { get; } 
            = new Dictionary<string, Entity>();

        public Dictionary<string,
            Dictionary<string, AttributeBuilder>> AttributeBuilders { get; } =
            new Dictionary<string, Dictionary<string, AttributeBuilder>>();

        public Dictionary<string,
            Dictionary<string, Attribute>> Attributes { get; } =
            new Dictionary<string, Dictionary<string, Attribute>>();

        
        public Dictionary<string,
            Dictionary<string, RelationBuilder>> RelationBuilders { get; } =
            new Dictionary<string, Dictionary<string, RelationBuilder>>();
        
        public Dictionary<string,
            Dictionary<string, IRelation>> Relations { get; } =
            new Dictionary<string, Dictionary<string, IRelation>>();
    }
}