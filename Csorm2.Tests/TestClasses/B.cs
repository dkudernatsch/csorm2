using Csorm2.Core.Attributes;

namespace Csorm2.Tests.TestClasses
{
    public class B
    {
        [PrimaryKey]
        public long Id { get; set; }
        
        [Column("b_data")]
        public string Data { get; set; }
        
        [ManyToOne(OtherEntity = typeof(A), OtherKey = "Id", ThisKey = "fk_a_id")]
        public A a { get; set; }
    }
}