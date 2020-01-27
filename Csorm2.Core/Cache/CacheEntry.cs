using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Csorm2.Core.Cache.ChangeTracker;
using Csorm2.Core.Extensions;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query.Delete;
using Csorm2.Core.Query.Expression;
using Csorm2.Core.Query.Insert;
using Attribute = Csorm2.Core.Metadata.Attribute;

namespace Csorm2.Core.Cache
{
    public class CacheEntry
    {
        public CacheEntry(Entity entity, object obj, DbContext context)
        {
            Object = obj;
            _context = context;
            Entity = entity;
        }

        private DbContext _context;
        public Entity Entity { get; }
        public object Object { get; }

        public object PrimaryKey => Entity.PrimaryKeyAttribute.InvokeGetter(Object);

        public Dictionary<string, object> OriginalValues { get; } = new Dictionary<string, object>();

        public Dictionary<string, object> ShadowAttributes { get; } = new Dictionary<string, object>();


        private Dictionary<string, ISet<object>> _relatedKeys = new Dictionary<string, ISet<object>>();

        public void AddRelatedKeys(Attribute attribute, IEnumerable<object> keys)
        {
            if (!attribute.IsEntityType)
                throw new Exception("Attribute must be a relation in order to associate it with related keys");
            var set = _relatedKeys.GetOrInsert(attribute.Name, new SortedSet<object>());
            set.UnionWith(keys);
        }

        public void ReplaceRelatedKeys(Attribute attribute, IEnumerable<object> keys)
        {
            if (!attribute.IsEntityType)
                throw new Exception("Attribute must be a relation in order to associate it with related keys");
            var set = _relatedKeys.GetOrInsert(attribute.Name, new SortedSet<object>());
            set.Clear();
            set.UnionWith(keys);
        }

        public IEnumerable<IValueChange> ValueChanges()
        {
            return Entity.Attributes.Values
                .Where(attr => !attr.IsShadowAttribute)
                .SelectMany(
                    attribute => attribute.Relation == null
                        ? ExtractSimpleChanges(attribute)
                        : ExtractRelationChanges(attribute)
                );
        }

        private IEnumerable<IValueChange> ExtractSimpleChanges(Attribute attribute)
        {
            var oldValue = OriginalValues[attribute.Name];
            var newValue = attribute.InvokeGetter(Object);
            if (Equals(oldValue, newValue)) return new IValueChange[0];

            if (Entity.PrimaryKeyAttribute == attribute)
                throw new ArgumentException(
                    "Primary key updates are not supported consider deleting the entity and inserting a new one");

            return new[] {new ValueChange(Entity, attribute, Object, oldValue, newValue)};
        }

        private IEnumerable<IValueChange> ExtractRelationChanges(Attribute attribute)
        {
            Trace.Assert(attribute.Relation != null);
            return attribute.Relation switch
            {
                OneToMany oneToMany => ExtractOneToManyChanges(oneToMany),
                ManyToOne manyToOne => ExtractManyToOneChanges(manyToOne),
                ManyToMany manyToMany => ExtractManyToManyChanges(manyToMany),
                _ => throw new NotSupportedException("Relation type is not supported")
            };
        }

        private IEnumerable<IValueChange> ExtractOneToManyChanges(OneToMany oneToMany)
        {
            var relatedKeys = _relatedKeys.GetOrInsert(oneToMany.FromEntityAttribute.Name, new HashSet<object>());
            var newRelatedObject = oneToMany.FromEntityAttribute.InvokeGetter(Object);
            var primaryKey = oneToMany.FromEntity.PrimaryKeyAttribute.InvokeGetter(Object);
            var changes = new List<IValueChange>();
            // was not loaded so no modification could be made
            if (newRelatedObject == null) return new IValueChange[0];
            if (newRelatedObject is IEnumerable newRelatedObjects)
            {
                var pks = newRelatedObjects.Cast<object>()
                    .Select(obj => (obj, oneToMany.ToEntity.PrimaryKeyAttribute.InvokeGetter(obj)))
                    .ToDictionary(tup => tup.Item2, tup => tup.obj);

                foreach (var (pk, obj) in pks)
                {
                    if (relatedKeys.Contains(pk)) continue;
                    if (_context.Cache.GetEntry(oneToMany.ToEntity, pk) == null)
                    {
                        _context.ChangeTracker.InsertNew(oneToMany.ToEntity, obj);
                        changes.Add(
                            new ValueChange(oneToMany.ToEntity, oneToMany.ToKeyAttribute, obj, null, primaryKey));
                    }
                    else
                    {
                        var fk = _context.Cache
                            .GetEntry(oneToMany.ToEntity, pk)
                            .ShadowAttributes[oneToMany.ToKeyAttribute.Name];

                        changes.Add(new ValueChange(oneToMany.ToEntity, oneToMany.ToKeyAttribute, obj, fk, primaryKey));
                    }
                }

                foreach (var key in relatedKeys)
                {
                    if (pks.TryGetValue(key, out _)) continue;

                    var entry = _context.Cache.GetEntry(oneToMany.ToEntity, key);
                    Trace.Assert(entry != null, "Tried to remove untracked entity");
                    changes.Add(new ValueChange(oneToMany.ToEntity, oneToMany.ToKeyAttribute, entry.Object, primaryKey,
                        null));
                }

                return changes;
            }

            throw new Exception("ManyToMany relation on non Enumerable value");
        }

        private IEnumerable<IValueChange> ExtractManyToOneChanges(ManyToOne manyToOne)
        {
            var oldForeignKey = ShadowAttributes.GetValueOrDefault(manyToOne.FromKeyAttribute.Name);
            var oldRelatedObject = OriginalValues.GetValueOrDefault(manyToOne.FromEntityAttribute.Name);
            var newRelatedObject = manyToOne.FromEntityAttribute.InvokeGetter(Object);

            // relation was not loaded and user did not interact with it
            if (newRelatedObject == null && oldRelatedObject == null) return new IValueChange[0];

            // new foreign key from newly set object
            var newForeignKey = newRelatedObject != null
                ? manyToOne.ToEntity.PrimaryKeyAttribute.InvokeGetter(newRelatedObject)
                : null;

            // relation was set to null => remove relation
            if (newRelatedObject == null && oldRelatedObject != null)
            {
                return new[] {new ValueChange(Entity, manyToOne.FromKeyAttribute, Object, oldForeignKey, null)};
            }

            //relation was changed or added
            var fromCacheRelatedObject = _context.Cache.GetEntry(manyToOne.ToEntity, newForeignKey);
            if (fromCacheRelatedObject == null)
            {
                //save new object
                _context.ChangeTracker.InsertNew(newRelatedObject);
                return new[]
                {
                    new DelayedValueChange(Entity, oldRelatedObject, newRelatedObject,
                        manyToOne.ToEntity.PrimaryKeyAttribute, manyToOne.FromKeyAttribute)
                };
            }

            return new IValueChange[0];
        }

        private IEnumerable<IValueChange> ExtractManyToManyChanges(ManyToMany manyToMany)
        {
            var manyToManyEntity = manyToMany.BetweenEntity;
            var pkLeft = PrimaryKey;
            var relatedKeys = _relatedKeys.GetValueOrDefault(manyToMany.FromEntityAttribute.Name);
            if (relatedKeys == null) return new IValueChange[0];
            var newValues = (IEnumerable<object>) manyToMany.FromEntityAttribute.InvokeGetter(Object);
            Trace.Assert(newValues != null,
                "related keys were loaded but relation value is null, should be empty list at worst");

            var newValueEntries = newValues
                .Select(obj =>
                    _context.Cache.GetEntry(manyToMany.ToEntity,
                        manyToMany.ToEntity.PrimaryKeyAttribute.InvokeGetter(obj))
                    ?? throw new Exception("Adding untracked entities to a many to many relation is not supported"))
                .ToDictionary(entry => entry.PrimaryKey);

            foreach (var entry in newValueEntries.Values)
            {
                if (relatedKeys.Contains(entry.PrimaryKey)) continue;

                var pkRight = entry.PrimaryKey;
                var insert = new InsertExpression<Dictionary<string, object>>(new[]
                {
                    new Dictionary<string, object>
                    {
                        {manyToMany.ReferencedFromAttribute.Name, pkLeft},
                        {manyToMany.ReferencedToAttribute.Name, pkRight}
                    }
                }, _context, manyToManyEntity);
                relatedKeys.Add(entry.PrimaryKey);
                _context.ChangeTracker.AddManyToMany(manyToManyEntity, insert);
            }
            var keysToRemove = new HashSet<object>();
            foreach (var key in relatedKeys)
            {
                if (newValueEntries.ContainsKey(key)) continue;

                var where = new WhereSqlFragment(
                    BinaryExpression.And(
                        BinaryExpression.Eq(
                            new Accessor
                            {
                                TableName = manyToMany.BetweenEntity.TableName,
                                PropertyName = manyToMany.ReferencedFromAttribute.DataBaseColumn
                            },
                            Value.FromAttr(PrimaryKey, manyToMany.ReferencedFromAttribute)
                        ), BinaryExpression.Eq(
                            new Accessor
                            {
                                TableName = manyToMany.BetweenEntity.TableName,
                                PropertyName = manyToMany.ReferencedToAttribute.DataBaseColumn
                            },
                            Value.FromAttr(key, manyToMany.ReferencedToAttribute)
                        ))
                );
                var delete = new DeleteQuery<Dictionary<string, object>>(_context, manyToMany.BetweenEntity, where);
                _context.ChangeTracker.RemoveManyToMany(Entity, delete);
                keysToRemove.Add(key);
            }
            foreach (var key in keysToRemove)
            {
                relatedKeys.Remove(key);
            }
            return new IValueChange[0];
        }
    }
}