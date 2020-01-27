using System;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Csorm2.Core;
using Csorm2.Core.Query;
using Csorm2.Core.Query.Expression;
using Csorm2.Core.Query.Insert;
using Csorm2.Tests.TestClasses;
using Newtonsoft.Json;
using Npgsql;
using Xunit;
using Xunit.Abstractions;
using static Csorm2.Core.Query.Expression.BinaryExpression;
using static Csorm2.Core.Query.Expression.Value;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace Csorm2.Tests.Schema
{
    public class TestContext
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestContext(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Test()
        {
            var ctx = new TestClasses.TestContext(
                () => new NpgsqlConnection("Host=localhost;Port=5432;User Id=user;Password=1234;Database=test_db"));

            //var newStudent1 = ctx.Students.Add(new Student {Name = "ABC"});
            //var newStudent2 = ctx.Students.Add(new Student {Name = "DEF"});
            var student = new Student {Name = "Daniel"};
            var insert = new InsertQueryBuilder(ctx)
                .Insert<Student>()
                .Value(student);
            ctx.Connection.Insert(insert);
            
            var gradeInsert = new InsertQueryBuilder(ctx)
                .Insert<Grade>()
                .Value(new Grade{Id = 2, Student = student});
            ctx.Connection.Insert(gradeInsert);
            
            
            var grade1 = ctx.Grades.Find(1L);
            var grade2 = ctx.Grades.Find(2L);
            var grade3 = ctx.Grades.Find(3L);
            
            var student1 = ctx.Students.Find(1L);
            var student2 = ctx.Students.Find(2L);

            //var newGrade = ctx.Grades.Add(new Grade {Id = 10, GradeValue = 50, Student = student1});

            var course = ctx.Courses.Find(1L);
                        
           // grade1.Student = newStudent1;

            grade2.Student = grade3.Student;
            grade2.GradeValue = 5;

            
           //  ctx.ChangeTracker.SaveChanges();
            
            //grade3.Student = newStudent2;
            
            //student2.Grades.Add(grade3);
            //_testOutputHelper.WriteLine(JsonConvert.SerializeObject(course.Students));
            
           // ctx.ChangeTracker.SaveChanges();

            _testOutputHelper.WriteLine(JsonConvert.SerializeObject(student1));
            _testOutputHelper.WriteLine(JsonConvert.SerializeObject(student2));
            //_testOutputHelper.WriteLine(JsonConvert.SerializeObject(s2));
        }
    }
}