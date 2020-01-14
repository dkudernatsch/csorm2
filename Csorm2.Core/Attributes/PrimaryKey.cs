using System;

namespace Csorm2.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class PrimaryKey: CsormAttribute
    {
        public PrimaryKey()
        {
        }
    }
}