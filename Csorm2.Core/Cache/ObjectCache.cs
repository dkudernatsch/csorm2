using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Csorm2.Core.Cache.ChangeTracker;
using Csorm2.Core.Metadata;

namespace Csorm2.Core.Cache
{
    public class ObjectCache
    {
        private readonly DbContext _ctx;

        private Dictionary<Entity, Dictionary<object, CacheEntry>> _objectCache = new Dictionary<Entity, Dictionary<object, CacheEntry>>();

        public bool EntityExists(Entity entity, object primaryKey)
        {
            return _objectCache[entity].ContainsKey(primaryKey);
        }

        public ObjectCache(DbContext ctx)
        {
            _ctx = ctx;
            foreach (var (entityName, entity) in _ctx.Schema.EntityNameMap)
            {
                _objectCache[entity] = new Dictionary<object, CacheEntry>();
            }
        }

        public CacheEntry GetEntry(Entity entity, object primaryKey) =>
            _objectCache[entity].GetValueOrDefault(primaryKey);


        internal CacheEntry GetOrInsertNew(Entity entity, object obj)
        {
            return GetEntry(entity, entity.PrimaryKeyAttribute.InvokeGetter(obj))
                   ?? InsertNew(entity, obj);
        }
        
        internal CacheEntry InsertNew(CacheEntry entry)
        {
            var oldEntry = entry.PrimaryKey == null 
                ? null 
                : _objectCache[entry.Entity].GetValueOrDefault(entry.PrimaryKey);
            
            if(oldEntry != null) throw new Exception("Can only insert _new_ cache entries");
            _objectCache[entry.Entity][entry.PrimaryKey] = entry;
            return entry;
        }

        public CacheEntry InsertNew(Entity entity, object obj)
        {
            var entry = new CacheEntry(entity, obj, _ctx);
            var pk = entity.PrimaryKeyAttribute.InvokeGetter(obj);
            _objectCache[entity][pk] = entry;
            return entry;
        }

        public IEnumerable<IValueChange> CollectChanges()
        {
            foreach (var (_, objects) in _objectCache)
            {
                foreach (var (pk, entity) in objects)
                {
                    foreach (var change in entity.ValueChanges())
                    {
                        yield return change;
                    }
                }
            }
        }
    }
}