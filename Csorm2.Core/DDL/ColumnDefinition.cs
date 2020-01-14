using System;
using System.Data;
using Csorm2.Core.Metadata;
using Attribute = Csorm2.Core.Metadata.Attribute;

namespace Csorm2.Core.DDL
{
    public class ColumnDefinition: IDdlStatement
    {
        private Attribute _attribute;
        private Entity _entity;
        
        private string Type => _attribute.IsAutoInc
            ? "bigserial"
            : DbTypeMap.AsDdl(_attribute.DatabaseType.Value);

        public ColumnDefinition(Entity entity, Attribute attribute)
        {
            _entity = entity;
            _attribute = attribute;
        }

        public string AsSqlString()
            => $"{_attribute.DataBaseColumn} {Type}" + (_entity.PrimaryKeyAttribute.Equals(_attribute) ? " primary key" : "");

    }
}