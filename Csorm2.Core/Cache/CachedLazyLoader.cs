using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Csorm2.Core.Extensions;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query;
using Csorm2.Core.Query.Expression;
using Csorm2.Core.Query.Select;
using BinaryExpression = Csorm2.Core.Query.Expression.BinaryExpression;

namespace Csorm2.Core.Cache
{
    public class CachedLazyLoader : ILazyLoader
    {
        private readonly Entity _entity;
        private readonly DbContext _context;
        private readonly CacheEntry _entry;
        private Dictionary<string, int> _attributeGenerations = new Dictionary<string, int>();

        public CachedLazyLoader(Entity entity, DbContext context, CacheEntry entry)
        {
            _entity = entity;
            _context = context;
            _entry = entry;
        }

        public ICollection<T> Load<T>(object entityObj, ref ICollection<T> loadTo, [CallerMemberName] string name = "")
        {
            if (!ShouldLoad(loadTo, name)) return loadTo;
            
            var propAttr = _entity.Attributes[name];
            var otherEntity = _context.Schema.EntityTypeMap[typeof(T)];
            if (propAttr.Relation is ManyToMany relation)
            {
                return LoadManyToMany(entityObj, ref loadTo, name, relation);
            }
            var attribute = _entity.Attributes[name];
            var fkAttr = attribute.Relation.ToKeyAttribute;
            var pkAttr = attribute.Relation.FromKeyAttribute;
            var pk = pkAttr.PropertyInfo.GetMethod.Invoke(entityObj, new object[] { });
            var otherPk = attribute.Relation.ToEntity.PrimaryKeyAttribute;
            var query = new QueryBuilder(_context)
                .Select<T>()
                .Where(new WhereSqlFragment(BinaryExpression.Eq(
                    new Accessor {PropertyName = fkAttr.DataBaseColumn, TableName = otherEntity.TableName},
                    Value.FromAttr(pk, pkAttr)
                ))).Build();
            var loaded = _context.Connection.Select(query).ToList();
            loadTo = loaded;
            _entry.RelationIdMap[name] =
                loaded.Select(obj => otherPk.PropertyInfo.GetMethod.Invoke(obj, new object[]{}))
                .ToHashSet();
            
            
            return loadTo;
        }

        public T Load<T>(object entityObj, ref T loadTo, [CallerMemberName] string name = "")
        {
            if (!ShouldLoad(loadTo, name)) return loadTo;
            
            var attribute = _entity.Attributes[name];
            var fkAttr = attribute.Relation.FromKeyAttribute;
            var pk = _entity.PrimaryKeyAttribute.PropertyInfo.GetMethod.Invoke(entityObj, new object[] { });

            var otherEntity = _context.Schema.EntityTypeMap[typeof(T)];

            var cacheEntry = _context.Cache.GetOrInsert(_entity, pk, entityObj);
            var fk = cacheEntry.OriginalEntity[fkAttr.Name];

            var query = new QueryBuilder(_context)
                .Select<T>()
                .Where(new WhereSqlFragment(
                    BinaryExpression.Eq(
                        new Accessor
                        {
                            PropertyName = attribute.Relation.ToKeyAttribute.DataBaseColumn,
                            TableName = otherEntity.TableName
                        },
                        Value.FromAttr(fk, fkAttr)
                    ))).Build();

            var loaded = _context.Connection.Select(query).First();
            loadTo = loaded;
            _entry.OriginalEntity[name] = loaded;
            _attributeGenerations[name] = _context.ChangeTracker.Generation;
            return loaded;
        }


        private ICollection<T> LoadManyToMany<T>(object entityObj, ref ICollection<T> loadTo, string propertyName,
            ManyToMany relation)
        {
            var propAttr = _entity.Attributes[propertyName];
            var otherEntity = _context.Schema.EntityTypeMap[typeof(T)];

            var thisEntity = _context.Schema.EntityTypeMap[typeof(T)];
            var betweenEntity = relation.BetweenEntity;
            //student.id
            var otherEntityPkAttr = relation.ToKeyAttribute;
            //fk_student
            var otherEntityFkAttr = relation.ReferencedToAttribute;

            //Students
            var fromEntityAttribute = relation.FromEntityAttribute;

            var pk = relation.FromKeyAttribute.PropertyInfo.GetMethod.Invoke(entityObj, new object[] { });

            var query = new SelectQueryBuilder(_context)
                .FromTable<T>(thisEntity.TableName)
                .Join(betweenEntity, otherEntityPkAttr, otherEntityFkAttr)
                .Select(otherEntity.Attributes.Values
                    .Where(attr => !attr.IsEntityType)
                    .Select(attr => attr.DataBaseColumn))
                .Where(new WhereSqlFragment(
                    BinaryExpression.Eq(new Accessor
                {
                    PropertyName = relation.ReferencedFromAttribute.DataBaseColumn, 
                    TableName = betweenEntity.TableName
                }, Value.FromAttr(pk, relation.FromKeyAttribute))))
                .Build();

            var loaded = _context.Connection.Select(query).ToList();
            loadTo = loaded;
            _entry.RelationIdMap[propertyName] = loaded.Select(obj =>
                otherEntity.PrimaryKeyAttribute.PropertyInfo.GetMethod.Invoke(obj, new object[] { })).ToHashSet();
            return loaded;
        }

        private bool ShouldLoad<T>(T loadTo, string name)
        {
            // most important otherwise change tracking doesnt work
            if (_context.ChangeTracker.IsCollectingChanges) return false;
            // if its null always load
            if (loadTo == null) return true;
            // data may be stale -> reload
            if (_attributeGenerations.GetOrInsert(name, -1) < _context.ChangeTracker.Generation) return true;
            //otherwise dont load
            return false;
        }
        
    }
}