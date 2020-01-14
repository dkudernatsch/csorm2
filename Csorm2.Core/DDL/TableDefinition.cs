using System;
using System.Collections.Generic;
using System.Linq;
using Csorm2.Core.Extensions;
using Csorm2.Core.Metadata;

namespace Csorm2.Core.DDL
{
    public class TableDefinition: IDdlStatement
    {
        private Entity _entity;

        public TableDefinition(Entity entity)
        {
            _entity = entity;
        }

        private IEnumerable<ColumnDefinition> ColumnDefintions => _entity.Attributes
            .Select(attr => attr.Value)
            .Where(attr => !attr.IsEntityType)
            .Select(attr => new ColumnDefinition(_entity, attr));
        

        public string AsSqlString()
        {
            return $"CREATE TABLE {_entity.TableName} \n" +
                   $"({string.Join(", \n", ColumnDefintions.Select(col => col.AsSqlString()))})";
        }

        public IEnumerable<IConstraint> Constraints()
        {
            return _entity.Attributes
                .FilterSelect(attr => attr.Value.Relation)
                .SelectMany(ForeignKeyDefinition.FromRelation);
        }
        
    }
}