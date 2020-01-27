using System.Collections.Generic;
using Csorm2.Core.Metadata;

namespace Csorm2.Core.Cache.ChangeTracker
{
    public class EntityChanges
    {
        public EntityChanges(IEnumerable<IValueChange> changes, Entity entity, object o)
        {
            Changes = new HashSet<IValueChange>(changes);
            Entity = entity;
            Object = o;
        }

        public Entity Entity { get; }
        public object Object { get; }

        public ISet<IValueChange> Changes { get; }
    }
}