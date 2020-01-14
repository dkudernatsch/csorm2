using Csorm2.Core.Metadata;

namespace Csorm2.Core.DDL
{
    public class ManyToManyConstraintTo : IConstraint
    {
        private ManyToMany _manyToMany;

        public ManyToManyConstraintTo(ManyToMany manyToMany)
        {
            _manyToMany = manyToMany;
        }

        public string OnTable => _manyToMany.BetweenEntity.TableName;
        public string Name => $"FK_{OnTable}_{_manyToMany.ToEntity.TableName}_{_manyToMany.FromEntity.TableName}";

        public string Definition => $"FOREIGN KEY({_manyToMany.ReferencedToAttribute.DataBaseColumn}) " +
                                    $"REFERENCES {_manyToMany.ToEntity.TableName}({_manyToMany.ToKeyAttribute.DataBaseColumn})";
    }


    public class ManyToManyConstraintFrom : IConstraint
    {
        private ManyToMany _manyToMany;

        public ManyToManyConstraintFrom(ManyToMany manyToMany)
        {
            _manyToMany = manyToMany;
        }

        public string OnTable => _manyToMany.BetweenEntity.TableName;
        public string Name => $"FK_{OnTable}_{_manyToMany.FromEntity.TableName}_{_manyToMany.ToEntity.TableName}";

        public string Definition => $"FOREIGN KEY({_manyToMany.ReferencedFromAttribute.DataBaseColumn}) " +
                                    $"REFERENCES {_manyToMany.FromEntity.TableName}({_manyToMany.FromKeyAttribute.DataBaseColumn})";
    }
}