using System;
using System.Collections.Generic;
using Csorm2.Core.Attributes;

namespace Csorm.SampleProject.Model
{
    [Table("Custom_name")]
    public class Teacher : Person
    {
        [Column("Custom_name")]
        public int Salary { get; set; }
        
        [OneToMany(OtherEntity = typeof(Course), ThisKey = "Id", OtherKey = "fk_Course_Teacher")]
        public List<Course> Courses { get; set; }
        
        [ManyToOne(OtherEntity = typeof(Class), OtherKey = "fk_class_teacher", ThisKey = "Id")]
        public List<Class> Classes { get; set; }
        
    }
}