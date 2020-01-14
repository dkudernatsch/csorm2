using Csorm2.Core.Metadata;

namespace Csorm2.Core.DDL
{
    public class OneToManyConstraint: IConstraint
    {
        private OneToMany _relation;

        public OneToManyConstraint(OneToMany relation)
        {
            _relation = relation;
        }
        public string OnTable => _relation.ToEntity.TableName;
        
        public string Name => $"FK_{OnTable}_{_relation.ToEntity.TableName}_{_relation.FromKeyAttribute.DataBaseColumn}";
        
        public string Definition => $"FOREIGN KEY({_relation.ToKeyAttribute.DataBaseColumn}) " +
                                    $"REFERENCES {_relation.FromEntity.TableName}({_relation.FromKeyAttribute.DataBaseColumn})";
    }
}