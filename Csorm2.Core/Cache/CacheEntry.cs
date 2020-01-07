using System.Collections.Generic;
using Csorm2.Core.Metadata;

namespace Csorm2.Core.Cache
{
    public class CacheEntry
    {
        public CacheEntry(Entity entity, object entityObject)
        {
            EntityObject = entityObject;
        }

        public object EntityObject { get; }
        public Dictionary<string, object> OriginalEntity = new Dictionary<string, object>();
        public Dictionary<string, ISet<object>> RelationIdMap = new Dictionary<string, ISet<object>>();
    }
}