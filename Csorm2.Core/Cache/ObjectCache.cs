using System.Collections.Generic;
using System.Collections.ObjectModel;
using Csorm2.Core.Extensions;
using Csorm2.Core.Metadata;

namespace Csorm2.Core.Cache
{
    public class ObjectCache
    {
        private readonly Dictionary<Entity, Dictionary<object, CacheEntry>> _existingEntityCache = new Dictionary<Entity, Dictionary<object, CacheEntry>>();

        public ReadOnlyDictionary<Entity, Dictionary<object, CacheEntry>> ObjectPool => new ReadOnlyDictionary<Entity, Dictionary<object, CacheEntry>>(_existingEntityCache);
        
        public CacheEntry GetOrInsert(Entity entity, object primaryKey, object empty)
        {
            var entityCache = _existingEntityCache.GetOrInsert(entity, new Dictionary<object, CacheEntry>());
            var cacheEntry = entityCache.GetOrInsert(primaryKey, new CacheEntry(entity, empty));
            return cacheEntry;
        }
        
        
    }
}