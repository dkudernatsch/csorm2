using System.Collections.Generic;
using Csorm2.Core.Attributes;
using Csorm2.Core.Cache;

namespace Csorm2.Tests.TestClasses
{
    public class Course
    {
        private ILazyLoader Lazy { get; set; } = new LazyLoader();

        [PrimaryKey] [Column("Id")] 
        public long? CourseId { get; set; }
        public string Name { get; set; }
        public string Room { get; set; }

        private ICollection<Student> _students;

        [ManyToMany(
            OtherEntity = typeof(Student),
            RelationTableName = "RelCourseStudent",
            ThisKey = "CourseId",
            OtherKey = "fk_course",
            OtherEntityThisKey = "Id",
            OtherEntityOtherKey = "fk_student"
        )]
        public ICollection<Student> Students
        {
            get => Lazy.Load(this, ref _students);
            set => _students = value;
        }
    }
}