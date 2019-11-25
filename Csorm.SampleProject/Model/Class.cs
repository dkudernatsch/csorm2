using System.Collections.Generic;
using Csorm2.Core.Attributes;

namespace Csorm.SampleProject.Model
{
    public class Class
    {
        [PrimaryKey]
        public long Id { get; set; }
        public string Name { get; set; }
        
        [OneToMany(OtherEntity = typeof(Student), OtherKey = "fk_student_class", ThisKey = "Id")]
        public List<Student> Students { get; set; }
        
        [ManyToOne(OtherEntity = typeof(Teacher), OtherKey = "Id", ThisKey = "fk_class_teacher")]
        public Teacher Teacher { get; set; }
        
    }
}