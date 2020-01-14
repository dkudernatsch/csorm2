using System;
using System.Collections.Generic;
using Csorm2.Core.Attributes;
using Csorm2.Core.Cache;
using Newtonsoft.Json;

namespace Csorm.SampleProject.Model
{
    public class Teacher : Person
    {
        private ILazyLoader Lazy { get; set; } = new LazyLoader();
        
        [Column("Custom_name")]
        public int Salary { get; set; }
        
        [JsonIgnore]
        [OneToMany(OtherEntity = typeof(Course), ThisKey = "Id", OtherKey = "fk_Course_Teacher")]
        public ICollection<Course> Courses { get => Lazy.Load(this, ref _courses); set => _courses = value; }
        private ICollection<Course> _courses;
        
        [JsonIgnore]
        [OneToMany(OtherEntity = typeof(Class), OtherKey = "fk_class_teacher", ThisKey = "Id")]
        public ICollection<Class> Classes { get => Lazy.Load(this, ref _classes); set => _classes = value; }
        private ICollection<Class> _classes;

    }
}