using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Csorm2.Core.Extensions;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query.Delete;
using Csorm2.Core.Query.Insert;
using Csorm2.Core.Query.Update;
using Attribute = Csorm2.Core.Metadata.Attribute;

namespace Csorm2.Core.Cache.ChangeTracker
{
    public class ChangeTracker
    {
        private DbContext _context;

        private readonly Dictionary<Entity, HashSet<object>> _newEntityCache = new Dictionary<Entity, HashSet<object>>();
        private readonly Dictionary<Entity, HashSet<object>> _deleteEntityCache = new Dictionary<Entity, HashSet<object>>();

        private readonly Dictionary<Entity,Changes> trackedChanges = new Dictionary<Entity, Changes>();
        
        public int Generation { get; private set; } = 0;
        
        public bool IsCollectingChanges { get; private set; } = false;

        public ChangeTracker(DbContext context)
        {
            _context = context;
        }

        public Dictionary<Entity, IEnumerable<Changes>> CollectChanges()
        {
            IsCollectingChanges = true;
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
            // changes from deletetion

            IsCollectingChanges = false;
            return dict;
        }

        private Changes ChangesFor(Entity entity, object primaryKey, CacheEntry entry,
            IEnumerable<Attribute> extractChangesFor)
        {
            var changes = new Changes(entity, entry.EntityObject, primaryKey);

            var valueChanges = extractChangesFor.SelectMany(attr => ExtractChanges(entity, attr, entry));

            foreach (var change in valueChanges)
            {
                if(change != null) changes.AddChange(change);
            }

            return changes.HasChanges() ? changes : null;
        }

        private IEnumerable<IValueChange> ExtractChanges(Entity e, Attribute attr, CacheEntry entry)
        {
            return attr.Relation == null
                ? (IEnumerable<IValueChange>) new[] {SimpleChanges(e, attr, entry)}
                : RelationChanges(e, attr, entry);
        }

        private List<IValueChange> RelationChanges(Entity e, Attribute attr, CacheEntry entry)
        {
            var oldFkVal = entry.OriginalEntity[attr.Relation.FromKeyAttribute.Name];
            var newRelationValue = attr.PropertyInfo.GetGetMethod().Invoke(entry.EntityObject, new object[] { });
            var oldRelationValue = entry.OriginalEntity.GetValueOrDefault(attr.Name);

            //was not loaded and user did not add something
            if (newRelationValue == null && oldRelationValue == null) return new List<IValueChange>();

            var newFkVal = newRelationValue != null
                ? attr.Relation.ToEntity.PrimaryKeyAttribute.PropertyInfo.GetMethod.Invoke(newRelationValue,
                    new object[] { })
                : null;

            //relation delete
            if (newRelationValue == null)
            {
                return new List<IValueChange> {new ValueChange(e, attr.Relation.FromKeyAttribute, oldFkVal, null)};
            }

            //relation changed/or added
            if (!Equals(oldFkVal, newFkVal))
            {
                var newEntity = _context.Cache.ObjectPool[attr.Relation.ToEntity].GetValueOrDefault(newFkVal);
                if (newEntity == null)
                {
                    _newEntityCache.GetOrInsert(attr.Relation.ToEntity, new HashSet<object>()).Add(newRelationValue);
                    return new List<IValueChange>
                    {
                        new DelayedValueChange(e, oldRelationValue, newRelationValue,
                            attr.Relation.ToEntity.PrimaryKeyAttribute, attr.Relation.FromKeyAttribute)
                    };
                }

                return new List<IValueChange> {new ValueChange(e, attr.Relation.FromKeyAttribute, oldFkVal, newFkVal)};
            }
            
            //was normally loaded and no changes were made
            if (Equals(oldFkVal, newFkVal))
            {
                return new List<IValueChange>();
            }
            
            throw new NotSupportedException($"Relationstate change of Entity {e.EntityName} not supported");
        }

        private static IValueChange SimpleChanges(Entity e, Attribute attr, CacheEntry entry)
        {
            var newVal = attr.PropertyInfo.GetGetMethod().Invoke(entry.EntityObject, new object[] { });
            var oldVal = entry.OriginalEntity[attr.Name];
            if (Equals(oldVal, newVal)) return null; // no changes

            if (e.PrimaryKeyAttribute.Name == attr.Name)
                throw new ArgumentException(
                    "Primary key updates are not supported consider deleting the entity and inserting a new one");

            return new ValueChange(e, attr, oldVal, newVal);
        }

        public T InsertNew<T>(T t)
        {
            var entity = _context.Schema.EntityTypeMap[typeof(T)];
            _newEntityCache.GetOrInsert(entity, new HashSet<object>()).Add(t);
            return t;
        }

        public T Delete<T>(T t)
        {
            var entity = _context.Schema.EntityTypeMap[typeof(T)];
            _deleteEntityCache.GetOrInsert(entity, new HashSet<object>()).Add(t);
            return t;
        }

        public void SaveChanges()
        {
            var changes = CollectChanges();
            var newEntities = _newEntityCache;
            var deleteEntities = _deleteEntityCache;
            
            var asInserts
                = newEntities.SelectMany(kv =>
                    kv.Value.Select(obj => new InsertQueryBuilder(_context).Insert(kv.Key).Value(obj)));

            var updates = changes.SelectMany(cs =>
                cs.Value.Select(c =>
                    new UpdateBuilder(_context).Update(cs.Key).SetValues(c.Obj, c.ChangesValues())));

            var deletes = deleteEntities.SelectMany(kv =>
                kv.Value.Select(obj => new DeleteQueryBuilder(_context).Delete(obj, kv.Key)));

            using var conn = _context.Connection;
            conn.BeginTransaction();

            foreach (var insert in asInserts)
            {
                _context.Connection.Insert(insert);
            }
            
            foreach (var update in updates)
            {
                _context.Connection.Update(update);
            }

            foreach (var delete in deletes)
            {
                _context.Connection.Delete(delete);
            }
            conn.Commit();
            Generation++;
            _newEntityCache.Clear();
            _deleteEntityCache.Clear();
        }


    }
}
