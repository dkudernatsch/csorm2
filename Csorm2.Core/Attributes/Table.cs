using System;

namespace csorm2_core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, 
        AllowMultiple = false, 
        Inherited = false)]
    public class Table: CsormAttribute
    {
        public Table(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; }
    }
}