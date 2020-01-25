using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Csorm2.Core.Attributes;
using Csorm2.Core.Cache.ChangeTracker;
using Csorm2.Core.Metadata;
using ManyToMany = Csorm2.Core.Metadata.ManyToMany;

namespace Csorm2.Core
{
    public class ChangeTrackedSet<TEntity>: ICollection<TEntity>
    {

        private readonly DbContext _context;
        private readonly ICollection<TEntity> _inner = new HashSet<TEntity>();
        private readonly IRelation _relation;
        
        private Entity Entity => _context.Schema.EntityTypeMap[typeof(TEntity)];
        
        public ChangeTrackedSet(DbContext context, IRelation relation)
        {
            _context = context;
            _relation = relation;
        }
        
        public bool Remove(TEntity item)
        {
            if (!Contains(item)) return false;
            if(_relation is ManyToMany) throw new Exception("Updating ManyToMany Relations is not supported");

            var pk = Entity.PrimaryKeyAttribute.InvokeGetter(item);
            var fkAttr = _relation.ToKeyAttribute;
            var entry = _context.Cache.ObjectPool[Entity][pk];
            _relation.ToEntityAttribute?.InvokeSetter(item, null);
            return _inner.Remove(item);
        }

        public void Add(TEntity item)
        {
            
            _inner.Add(item);
        }

        public void Clear()
        {
            
            _inner.Clear();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        public bool Contains(TEntity item)
        {
            return _inner.Contains(item);
        }

        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            _inner.CopyTo(array, arrayIndex);
        }

        public int Count => _inner.Count;
        public bool IsReadOnly => _inner.IsReadOnly;
    }
}