using System.Collections.Generic;
using Csorm2.Core.Extensions;
using Csorm2.Core.Metadata;

namespace Csorm2.Core.Cache
{
    public class ObjectCache
    {
        private readonly Dictionary<Entity, Dictionary<object, CacheEntry>> _existingEntityCache = new Dictionary<Entity, Dictionary<object, CacheEntry>>();
        private readonly Dictionary<Entity, List<object>> _newEntityCache = new Dictionary<Entity, List<object>>();


        public CacheEntry GetOrInsert(Entity entity, object primaryKey, object empty)
        {
            var entityCache = _existingEntityCache.GetOrInsert(entity, new Dictionary<object, CacheEntry>());
            var cacheEntry = entityCache.GetOrInsert(primaryKey, new CacheEntry(entity, empty));
            return cacheEntry;
        }
        
        
    }
}