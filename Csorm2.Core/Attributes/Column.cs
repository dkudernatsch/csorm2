using System;

namespace csorm2_core.Attributes
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