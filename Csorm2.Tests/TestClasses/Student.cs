using System.Collections.Generic;
using Csorm2.Core.Attributes;
using Csorm2.Core.Cache;

namespace Csorm2.Tests.TestClasses
{
    public class Student
    {
        private ILazyLoader Lazy { get; set; } = new LazyLoader();

        public Student(){}
        [PrimaryKey]
        [AutoIncrement]
        public long Id { get; set; }
        public string Name { get; set; }

        private ICollection<Grade> _grades;

        [OneToMany(OtherEntity = typeof(Grade), ThisKey = "Id", OtherKey = "FK_Grade_Student")]
        public ICollection<Grade> Grades
        {
            get => Lazy.Load(this, ref _grades); 
            set => _grades = value;
        }
    }
}