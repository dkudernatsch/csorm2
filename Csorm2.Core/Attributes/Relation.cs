using System;

namespace Csorm2.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class Relation: CsormAttribute 
    {

        public string ThisKey { get; set; }
        public string OtherKey { get; set; }
        public Type OtherEntity { get; set; }
    }
}