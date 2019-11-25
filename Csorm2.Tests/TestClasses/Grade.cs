using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Csorm2.Core.Attributes;
using Csorm2.Core.Cache;

namespace Csorm2.Tests.TestClasses
{
    public class Grade
    {
        private ILazyLoader Lazy { get; set; }
        
        [PrimaryKey]
        public int Id { get; set; }

        public int GradeValue { get; set; }

        private Student _student;
        
        [JsonIgnore]
        [IgnoreDataMember]
        [ManyToOne(OtherEntity = typeof(Student), ThisKey = "FK_Grade_Student", OtherKey = "Id")]
        public Student Student {
            get => Lazy.Load(this, ref _student); 
            set => _student = value; 
        }
    }
}