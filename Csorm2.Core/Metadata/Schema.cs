using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Csorm2.Core.Metadata
{
    /// <summary>
    /// Represents Database schema
    /// </summary>
    public class Schema
    {
        public Schema()
        {
            EntityNameMap = new ReadOnlyDictionary<string, Entity>(_entityMap);
            EntityTypeMap = new ReadOnlyDictionary<Type, Entity>(_entityTypeMap);
        }

        private Dictionary<string, Entity> _entityMap = new Dictionary<string, Entity>();
        private Dictionary<Type, Entity> _entityTypeMap = new Dictionary<Type, Entity>();
        /// <summary>
        /// readonly view of all tracked entity types by name
        /// </summary>
        public IReadOnlyDictionary<string, Entity> EntityNameMap { get; }
        /// <summary>
        /// readonly view of all tracked entity types by type
        /// </summary>
        public IReadOnlyDictionary<Type, Entity> EntityTypeMap { get; }
        
        
        /// <summary>
        /// Adds a new entity type
        /// </summary>
        /// <param name="e"></param>
        public void AddEntity(Entity e)
        {
            _entityMap.Add(e.EntityName, e);
            if(e.ClrType != null) _entityTypeMap.Add(e.ClrType, e);
        }
    }
}