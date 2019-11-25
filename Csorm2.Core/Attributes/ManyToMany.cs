using System;

namespace csorm2_core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ManyToMany: Relation
    {
        public ManyToMany(string relationTableName)
        {
            RelationTableName = relationTableName;
        }

        public string RelationTableName { get; }
    }
}