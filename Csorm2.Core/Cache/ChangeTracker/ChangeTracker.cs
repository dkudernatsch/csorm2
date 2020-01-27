using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Csorm2.Core.Extensions;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query.Delete;
using Csorm2.Core.Query.Insert;
using Csorm2.Core.Query.Update;

namespace Csorm2.Core.Cache.ChangeTracker
{
    public class ChangeTracker
    {
        private DbContext _context;
        
        public uint Generation { get; private set; } = 0;

        private readonly Dictionary<Entity, HashSet<object>> _newEntityCache = new Dictionary<Entity, HashSet<object>>();
        
        private readonly Dictionary<Entity, List<InsertExpression<Dictionary<string, object>>>> _newManyToManys
            = new Dictionary<Entity, List<InsertExpression<Dictionary<string, object>>>>();
        
        private readonly Dictionary<Entity, List<DeleteQuery<Dictionary<string, object>>>> _removeManyToManys 
            = new Dictionary<Entity, List<DeleteQuery<Dictionary<string, object>>>>();
        
        private readonly Dictionary<Entity, HashSet<object>> _deletedEntityCache = new Dictionary<Entity, HashSet<object>>();

        public bool IsCollectingChanges { get; set; } = false;

        public ChangeTracker(DbContext context)
        {
            _context = context;
        }

        public Dictionary<Entity, IEnumerable<EntityChanges>> CollectChanges()
        {
            IsCollectingChanges = true;

            var changes =_context.Cache.CollectChanges();
            var collectedChanges = changes.GroupBy(change => change.EntityObj)
                .Select(group => new EntityChanges(group, group.First().Entity, group.Key))
                .GroupBy(ec => ec.Entity)
                .ToDictionary(group => group.Key, group => group.AsEnumerable());
            
            IsCollectingChanges = false;
            return collectedChanges;
        }

        public void SaveChanges()
        {
            var changes = CollectChanges();
            
            var inserts = _newEntityCache
                .SelectMany(kv => kv.Value
                    .Select(obj => new InsertQueryBuilder(_context).Insert(kv.Key).Value(obj)));

            var updates = changes.SelectMany(cs =>
                cs.Value.Select(c => new UpdateBuilder(_context).Update(cs.Key).SetValues(c.Object, c.Changes)));

            var deletes = _deletedEntityCache.SelectMany(kv =>
                kv.Value.Select(obj => new DeleteQueryBuilder(_context).Delete(obj, kv.Key)));

            using var conn = _context.Connection;
            conn.BeginTransaction();
            
            foreach (var insert in inserts)
            {
                _context.Connection.Insert(insert);
            }

            foreach (var newManyToMany in _newManyToManys.Values.SelectMany(s => s))
            {
                _context.Connection.InsertStatement(newManyToMany);
            }
            
            foreach (var update in updates)
            {
                _context.Connection.Update(update);
            }
            
            foreach (var delete in deletes)
            {
                _context.Connection.Delete(delete);
            }
            
            foreach (var delete in _removeManyToManys.Values.SelectMany(s => s))
            {
                _context.Connection.Delete(delete);
            }
            
            conn.Commit();
            Generation++;
            
            _newEntityCache.Clear();
            _newManyToManys.Clear();
            _removeManyToManys.Clear();
            _deletedEntityCache.Clear();
            
        }
        
        public T InsertNew<T>(T t)
        {
            var entity = _context.Schema.EntityTypeMap[typeof(T)];
            _newEntityCache.GetOrInsert(entity, new HashSet<object>()).Add(t);
            return t;
        }
        
        public object InsertNew(Entity entity, object obj)
        {
            _newEntityCache.GetOrInsert(entity, new HashSet<object>()).Add(obj);
            return obj;
        }

        public T Delete<T>(T t)
        {
            var entity = _context.Schema.EntityTypeMap[typeof(T)];
            _deletedEntityCache.GetOrInsert(entity, new HashSet<object>()).Add(t);
            return t;
        }

        public void AddManyToMany(Entity manyToManyEntity, InsertExpression<Dictionary<string, object>> manyToManyInsert)
        {
            _newManyToManys.GetOrInsert(manyToManyEntity, new List<InsertExpression<Dictionary<string, object>>>())
                .Add(manyToManyInsert);
        }

        public void RemoveManyToMany(Entity manyToManyEntity, DeleteQuery<Dictionary<string, object>> manyToManyDelete)
        {
            _removeManyToManys.GetOrInsert(manyToManyEntity, new List<DeleteQuery<Dictionary<string, object>>>())
                .Add(manyToManyDelete);
        }
    }
}