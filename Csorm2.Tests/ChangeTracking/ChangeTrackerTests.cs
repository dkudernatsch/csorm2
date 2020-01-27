using Csorm2.Tests.TestClasses;
using Npgsql;
using Xunit;

namespace Csorm2.Tests.Schema
{
    [Collection("IntegrationTest")]
    public class ChangeTrackerTests
    {
        [Fact]
        public void TestInsert()
        {
            using (var ctx = new TestClasses.TestContext(
                () => new NpgsqlConnection("Host=localhost;Port=5432;User Id=user;Password=1234;Database=test_db")))
            {
                ctx.EnsureRecreated();

                var student1 = ctx.Students.Add(new Student {Name = "stud1"});
                var student2 = ctx.Students.Add(new Student {Name = "stud2"});
                var student3 = ctx.Students.Add(new Student {Name = "stud3"});

                ctx.SaveChanges();
            }
        
            using (var ctx = new TestClasses.TestContext(
                () => new NpgsqlConnection("Host=localhost;Port=5432;User Id=user;Password=1234;Database=test_db")))
            {
                var student1 = ctx.Students.Find(1L);
                var student2 = ctx.Students.Find(2L);
                var student3 = ctx.Students.Find(3L);

                Assert.Equal(1,student1.Id);
                Assert.Equal("stud1",student1.Name);
                
                Assert.Equal(2,student2.Id);
                Assert.Equal("stud2",student2.Name);
                
                Assert.Equal(3,student3.Id);
                Assert.Equal("stud3",student3.Name);
            }   
            
        }
        
    }
}