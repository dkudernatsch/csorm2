using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Csorm2.Core.Extensions;
using Csorm2.Core.Metadata;
using Attribute = Csorm2.Core.Metadata.Attribute;

namespace Csorm2.Core.Cache.ChangeTracker
{
    public class ChangeTracker
    {
        private DbContext _context;
        
        private readonly Dictionary<Entity, List<object>> _newEntityCache = new Dictionary<Entity, List<object>>();
        private readonly Dictionary<Entity, Dictionary<object, Changes>> trackedChanges = new Dictionary<Entity, Dictionary<object, Changes>>();
        
        
        
        public ChangeTracker(DbContext context)
        {
            _context = context;
        }

        public Dictionary<Entity, IEnumerable<Changes>> CollectChanges()
        {
            var dict = new Dictionary<Entity, IEnumerable<Changes>>();
            // for each entity type
            foreach (var (entity, entities) in _context.Cache.ObjectPool)
            {
                // all attributes that are directly stored as a column in the database
                var valueAttributes = entity.Attributes
                    .Select(attr => attr.Value)
                    .Where(attr => !attr.IsShadowAttribute)
                    .Where(attr => attr.PropertyInfo?.GetMethod != null)
                    .ToList();

                var changes = entities
                    .FilterSelect(entry => ChangesFor(entity, entry.Key, entry.Value, valueAttributes))
                    .ToList();
                
                if (changes.Count > 0) 
                    dict[entity] = changes;
                
            }
            return dict;
        }

        private Changes ChangesFor(Entity entity, object primaryKey, CacheEntry entry, IEnumerable<Attribute> extractChangesFor)
        {
            
            var changes = new Changes(entity, primaryKey, entry);
            
            var valueChanges = extractChangesFor.FilterSelect(attr => ExtractChanges(entity, attr, entry));
                    
            foreach (var change in valueChanges)
            {
                changes.AddChange(change);
            }

            return changes.HasChanges() ? changes : null;
            
        }
        
        private ValueChange ExtractChanges(Entity e, Attribute attr, CacheEntry entry)
        {
            return attr.Relation == null ? 
                SimpleChanges(e, attr, entry) : 
                RelationChanges(e, attr, entry);
        }

        private ValueChange RelationChanges(Entity e, Attribute attr, CacheEntry entry)
        {
            var newRelationValue = attr.PropertyInfo.GetGetMethod().Invoke(entry.EntityObject, new object[] { });
            if (newRelationValue == null) return null;
            
            
        }
        
        private static ValueChange SimpleChanges(Entity e, Attribute attr, CacheEntry entry)
        {
            var newVal = attr.PropertyInfo.GetGetMethod().Invoke(entry.EntityObject, new object[] { });
            var oldVal = entry.OriginalEntity[attr.Name];
            if (newVal == oldVal) return null; // no changes

            if (e.PrimaryKeyAttribute.Name == attr.Name)
                throw new ArgumentException(
                    "Primary key updates are not supported consider deleting the entity and inserting a new one");

            return new ValueChange(attr, oldVal, newVal);
        }
    }
}