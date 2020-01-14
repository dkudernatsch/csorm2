using System;
using System.Collections.Generic;
using Csorm2.Core.Attributes;
using Csorm2.Core.Cache;
using Newtonsoft.Json;

namespace Csorm.SampleProject.Model
{
    public class Student : Person
    {
        private ICollection<Course> _courses;
        private Class _class;
        private ILazyLoader Lazy { get; set; } = new LazyLoader();

        [JsonIgnore]
        [ManyToMany(
            OtherEntity = typeof(Course),
            RelationTableName = "StudentCourseRel",
            OtherKey = "fK_student_id",
            OtherEntityOtherKey = "fk_course_id",
            ThisKey = "Id",
            OtherEntityThisKey = "OtherNameThanId")]
        public ICollection<Course> Courses
        {
            get => Lazy.Load(this, ref _courses);
            set => _courses = value;
        }
        [JsonIgnore]
        [ManyToOne(OtherEntity = typeof(Class), OtherKey = "Id", ThisKey = "fk_student_class")]
        public Class Class
        {
            get => Lazy.Load(this, ref _class);
            set => _class = value;
        }
    }
}