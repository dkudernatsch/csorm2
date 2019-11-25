using System;
using System.Collections.Generic;
using Csorm2.Core.Attributes;

namespace Csorm.SampleProject.Model
{
    public class Student : Person
    {
        [ManyToMany(OtherEntity = typeof(Course), RelationTableName = "StudentCourseRel", OtherKey = "fK_student_id", OtherEntityOtherKey = "fk_course_id", ThisKey = "Id", OtherEntityThisKey = "OtherNameThanId")]
        public List<Course> Courses { get; set; }

        [ManyToOne(OtherEntity = typeof(Class), OtherKey = "Id", ThisKey = "fk_student_class")] 
        public Class Class { get; set; }
        
    }
}