using System;
using Csorm2.Core.Attributes;

namespace Csorm.SampleProject.Model
{
    public class Person
    {
        [AutoIncrement]
        [PrimaryKey]
        public long? Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; } 
        public DateTime BDay { get; set; }

    }
}