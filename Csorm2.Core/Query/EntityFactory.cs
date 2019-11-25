using System;
using csorm_core.CSORM.Metadata;

namespace csorm_core.CSORM.Query
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
            return _entity.EntityType.GetConstructor(new Type[]{})?.Invoke(new object[]{});
        }
    }
}