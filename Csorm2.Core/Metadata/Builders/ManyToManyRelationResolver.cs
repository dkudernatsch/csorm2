using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Csorm2.Core.Metadata.Builders
{
    public static class ManyToManyRelationResolver
    {
        public static IEnumerable<EntityBuilder> ScanManyToManyRelation(SchemaBuildContext context)
        {
            return context.EntityBuilders.Values
                .Where(eb => eb.ClrType != null)
                .SelectMany(eb => AttributesFromTypeResolver.ValidAttributesFromType(eb.ClrType))
                .Select(attr => attr.GetCustomAttribute(typeof(Attributes.ManyToMany)) as Attributes.ManyToMany)
                .Where(r => r != null)
                .Select(r => new EntityBuilder(context).FromManyToManyRelation(r));
        } 
    }
}