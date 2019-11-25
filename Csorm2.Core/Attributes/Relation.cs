using System;

namespace csorm2_core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class Relation: CsormAttribute 
    {

        public string? ThisKey { get; set; }
        public string? OtherKey { get; set; }
        public Type? OtherEntity { get; set; }
    }
}