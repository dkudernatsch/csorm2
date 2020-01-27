using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Csorm2.Core.Metadata;
using Attribute = Csorm2.Core.Metadata.Attribute;

namespace Csorm2.Core.Cache
{
    public class CachedDataReader: IDisposable
    {
        public CachedDataReader( ObjectCache cache, Entity entity, IDataReader dataReader, DbContext context)
        {
            Entity = entity;
            DataReader = dataReader;
            _context = context;
            Cache = cache;
        }

        private DbContext _context;
        private ObjectCache Cache;
        private Entity Entity { get; }
        private IDataReader DataReader { get; }

        public IEnumerable<CacheEntry> ReadAllInto(IEnumerable<object> objects)
        {
            using var objectEnum = objects.GetEnumerator(); 
            
            while (DataReader.Read() && objectEnum.MoveNext())
            {
                var obj = objectEnum.Current;
                var entry = ReadIntoEntry(new CacheEntry(Entity, obj, _context));
                yield return Cache.InsertNew(entry);
            }
        }
        
        public IEnumerable<CacheEntry> ReadAll()
        {
            while (DataReader.Read())
            {
                var pk = DataReader[Entity.PrimaryKeyAttribute.Name];
                var oldEntry = Cache.GetEntry(Entity, pk);
                if (oldEntry != null)
                {
                    yield return ReadIntoEntry(oldEntry);
                }
                else
                {
                    var obj = ObjectProvider.Construct(Entity.ClrType);
                    Entity.PrimaryKeyAttribute.InvokeSetter(obj, pk);
                    
                    yield return ReadIntoEntry(Cache.InsertNew(
                        new CacheEntry(Entity, obj, _context)
                    ));
                }
            }
        }
        
        private CacheEntry ReadIntoEntry(CacheEntry entry)
        {
            foreach (var attribute in Entity.Attributes.Values)
            {
                if (attribute.IsEntityType)
                {
                    _context.ChangeTracker.IsCollectingChanges = true;
                    var value = attribute.InvokeGetter(entry.Object);
                    _context.ChangeTracker.IsCollectingChanges = false;
                    if (value != null) entry.OriginalValues[attribute.Name] = value;
                }
                else
                {
                    var value = DataReader[attribute.DataBaseColumn];
                    if (attribute.IsShadowAttribute)
                    {
                        entry.ShadowAttributes[attribute.Name] = value;
                    }
                    else
                    {
                        entry.OriginalValues[attribute.Name] = value;
                        attribute.InvokeSetter(entry.Object, value);
                    }
                }
                
            }
            
            return entry;
        }
        
        
        public void Dispose()
        {
            DataReader.Dispose();
        }
    }
}