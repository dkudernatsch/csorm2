using System.Collections.Generic;
using Csorm2.Core.Attributes;

namespace Csorm.SampleProject.Model
{
    public class Course
    {
        [PrimaryKey]
        public long OtherNameThanId { get; set; }
        
        public bool Active { get; set; }
        
        public string Name { get; set; }
        
        [ManyToOne(OtherEntity = typeof(Teacher), OtherKey = "Id", ThisKey = "fk_Course_Teacher")]
        public Teacher Teacher { get; set; }
        
        [ManyToMany(OtherEntity = typeof(Student), RelationTableName = "StudentCourseRel", OtherEntityOtherKey = "fK_student_id", OtherKey = "fk_course_id", ThisKey = "OtherNameThanId", OtherEntityThisKey = "Id")]
        public List<Student> Students { get; set; }
        
    }
}