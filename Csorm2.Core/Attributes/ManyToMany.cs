using System;

namespace Csorm2.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ManyToMany : Relation
    {
        public string RelationTableName { get; set; }
        public string OtherEntityThisKey { get; set; }
        public string OtherEntityOtherKey { get; set; }
    }
}