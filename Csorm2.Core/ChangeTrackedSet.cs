using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Csorm2.Core.Cache.ChangeTracker;

namespace Csorm2.Core
{
    public class ChangeTrackedSet<TEntity>: ICollection<TEntity>
    {

        private readonly ChangeTracker _tracker;
        private readonly HashSet<TEntity> _inner = new HashSet<TEntity>();

        public ChangeTrackedSet(ChangeTracker changeTracker)
        {
            _tracker = changeTracker;
        }
        
        public bool Remove(TEntity item)
        {
            throw new System.NotImplementedException();
        }

        public void Add(TEntity item)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }
        
        #region MyRegion
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<TEntity> GetEnumerator() => _inner.GetEnumerator();
        public bool Contains(TEntity item) => _inner.Contains(item);
        public void CopyTo(TEntity[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);
        public int Count => _inner.Count;
        public bool IsReadOnly => false;
        #endregion
    }
}