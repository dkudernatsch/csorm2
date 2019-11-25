using System;
using Csorm2.Core.Metadata;

namespace Csorm2.Core.Query
{
    public class EntityFactory
    {
        private Entity _entity;
        private DbContext _ctx;

        public EntityFactory(DbContext ctx, Entity entity)
        {
            _ctx = ctx;
            _entity = entity;
        }
        
        public object Create()
        {
            return _entity.ClrType.GetConstructor(new Type[]{})?.Invoke(new object[]{});
        }
    }
}