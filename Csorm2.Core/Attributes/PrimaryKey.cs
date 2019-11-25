using System;

namespace csorm2_core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKey: Column
    {
        public PrimaryKey(string columnName) : base(columnName)
        {
        }
    }
}