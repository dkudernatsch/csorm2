using System;

namespace Csorm2.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKey: CsormAttribute
    {
        public PrimaryKey()
        {
        }
    }
}