using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Csorm2.Core.Attributes;
using Csorm2.Core.Cache;

namespace Csorm2.Tests.TestClasses
{
    public class Grade
    {
        private ILazyLoader Lazy { get; set; } = new LazyLoader();
        
        [PrimaryKey]
        public long? Id { get; set; }

        public int GradeValue { get; set; }

        private Student _student;
        
        [IgnoreDataMember]
        [ManyToOne(OtherEntity = typeof(Student), ThisKey = "FK_Grade_Student", OtherKey = "Id")]
        public Student Student {
            get => Lazy.Load(this, ref _student); 
            set => _student = value; 
        }
    }
}