using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Csorm2.Core.Attributes;

namespace Csorm2.Tests.TestClasses
{
    [Table("t_a")]
    public class A
    {
        [PrimaryKey] 
        [Column("a_id")] 
        public long Id { get; set; }
        
        public string TestData { get; set; }

        [OneToMany(OtherEntity = typeof(B), ThisKey = "Id", OtherKey = "fk_a_id")]
        public List<B> Bs { get; set; }

        [ManyToMany(
            RelationTableName = "AsToBs",
            OtherEntity = typeof(B),
            ThisKey = "Id",
            OtherKey = "fk_a_id",
            OtherEntityThisKey = "Id",
            OtherEntityOtherKey = "fk_b_id"
        )]
        public List<B> Bss { get; set; }
    }
}