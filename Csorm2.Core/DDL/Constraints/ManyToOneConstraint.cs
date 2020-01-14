using Csorm2.Core.Metadata;

namespace Csorm2.Core.DDL
{
    public class ManyToOneConstraint : IConstraint
    {
        private ManyToOne _relation;

        public ManyToOneConstraint(ManyToOne relation)
        {
            _relation = relation;
        }

        public string OnTable => _relation.FromEntity.TableName;

        public string Name =>
            $"FK_{OnTable}_{_relation.FromEntity.TableName}_{_relation.ToKeyAttribute.DataBaseColumn}";

        public string Definition => $"FOREIGN KEY({_relation.FromKeyAttribute.DataBaseColumn}) " +
                                    $"REFERENCES {_relation.ToEntity.TableName}({_relation.ToKeyAttribute.DataBaseColumn})";
    }
}