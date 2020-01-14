using System.Collections.Generic;
using Csorm2.Core.Attributes;
using Csorm2.Core.Cache;

namespace Csorm.SampleProject.Model
{
    public class Course
    {
        private Teacher _teacher;
        private ICollection<Student> _students;

        private ILazyLoader Lazy { get; set; } = new LazyLoader();


        [AutoIncrement]
        [PrimaryKey]
        public long? OtherNameThanId { get; set; }
        
        public bool Active { get; set; }
        
        public string Name { get; set; }

        [ManyToOne(OtherEntity = typeof(Teacher), OtherKey = "Id", ThisKey = "fk_Course_Teacher")]
        public Teacher Teacher
        {
            get => Lazy.Load(this, ref _teacher);
            set => _teacher = value;
        }

        [ManyToMany(OtherEntity = typeof(Student), RelationTableName = "StudentCourseRel",
            OtherEntityOtherKey = "fK_student_id", OtherKey = "fk_course_id", ThisKey = "OtherNameThanId",
            OtherEntityThisKey = "Id")]
        public ICollection<Student> Students
        {
            get => Lazy.Load(this, ref _students);
            set => _students = value;
        }
    }
}