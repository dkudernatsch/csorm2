using Csorm2.Core.Metadata;

namespace Csorm2.Core.DDL
{
    public class PrimaryKeyConstraint: IConstraint
    {
        private Entity _entity;

        public PrimaryKeyConstraint(Entity entity)
        {
            _entity = entity;
        }

        public string OnTable => _entity.TableName;
        public string Name => $"PK_{_entity.TableName}_{_entity.PrimaryKeyAttribute.DataBaseColumn}";
        public string Definition => $"PRIMARY KEY({_entity.PrimaryKeyAttribute.DataBaseColumn})";
    }
}