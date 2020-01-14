using System.Collections.Generic;
using Csorm2.Core.Attributes;
using Csorm2.Core.Cache;
using Newtonsoft.Json;

namespace Csorm.SampleProject.Model
{
    public class Class
    {
        private ICollection<Student> _students;
        private Teacher _teacher;
        private ILazyLoader Lazy { get; set; } = new LazyLoader();

        
        [PrimaryKey]
        [AutoIncrement]
        public long? Id { get; set; }
        public string Name { get; set; }
        
        [JsonIgnore]
        [OneToMany(OtherEntity = typeof(Student), OtherKey = "fk_student_class", ThisKey = "Id")]
        public ICollection<Student> Students
        {
            get => Lazy.Load(this, ref _students);
            set => _students = value;
        }

        [ManyToOne(OtherEntity = typeof(Teacher), OtherKey = "Id", ThisKey = "fk_class_teacher")]
        public Teacher Teacher
        {
            get => Lazy.Load(this, ref _teacher);
            set => _teacher = value;
        }
    }
}