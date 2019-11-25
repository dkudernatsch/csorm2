using System;

namespace Csorm2.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Column: CsormAttribute
    {
        public Column(string columnName)
        {
            ColumnName = columnName;
        }

        public string ColumnName { get; }
    }
}