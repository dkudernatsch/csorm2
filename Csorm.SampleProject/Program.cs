using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Csorm.SampleProject.Model;
using Csorm2.Core;
using Csorm2.Core.Metadata.Builders;
using Csorm2.Core.Query;
using Csorm2.Core.Query.Expression;
using Csorm2.Core.Query.Select;
using Newtonsoft.Json;
using Npgsql;

namespace Csorm.SampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var ctx = new SampleDbContext(
                () => new NpgsqlConnection("Host=localhost;Port=5432;User Id=user;Password=1234;Database=test_db"),
                true);
            InsertData(ctx);
            ShowNavigation(ctx);
            Updates(ctx);
            Console.WriteLine();
        }

        private static void Updates(SampleDbContext ctx)
        {
            var teacher = ctx.Teachers.Find(1L);
            var course = ctx.Courses.Where(new WhereSqlFragment(
                BinaryExpression.Eq(
                    new Accessor{PropertyName = "Name", TableName = "Course"}, Value.String("SWE3", "name")
                    )
                )).First();
            teacher.Salary += 1_000;
            course.Active = false;
            ctx.SaveChanges();
        }

        private static void ShowNavigation(SampleDbContext ctx)
        {
            var student = ctx.Students.Find(1L);
            Console.WriteLine(JsonConvert.SerializeObject(student.Class));
            Console.WriteLine(JsonConvert.SerializeObject(student.Class.Students));
            Console.WriteLine(JsonConvert.SerializeObject(student.Class.Teacher.Classes));
            
        }

        private static void InsertData(SampleDbContext ctx)
        {
            var teacher1 = ctx.Teachers.Add(new Teacher
                {BDay = DateTime.Now, FirstName = "abc1", Name = "def1", Salary = 3500});
            var teacher2 = ctx.Teachers.Add(new Teacher
                {BDay = DateTime.Now, FirstName = "abc2", Name = "def2", Salary = 3500});
            var teacher3 = ctx.Teachers.Add(new Teacher
                {BDay = DateTime.Now, FirstName = "abc3", Name = "def3", Salary = 3500});
            var teacher4 = ctx.Teachers.Add(new Teacher
                {BDay = DateTime.Now, FirstName = "abc4", Name = "def4", Salary = 3500});
            ctx.SaveChanges();

            var class1 = ctx.Classes.Add(new Class {Name = "BIF1", Teacher = teacher1});
            var class2 = ctx.Classes.Add(new Class {Name = "BIF2", Teacher = teacher1});
            var class3 = ctx.Classes.Add(new Class {Name = "BIF3", Teacher = teacher2});
            var class4 = ctx.Classes.Add(new Class {Name = "BIF4", Teacher = teacher3});

            var course1 = ctx.Courses.Add(new Course {Active = true, Name = "SWE3", Teacher = teacher1});
            var course2 = ctx.Courses.Add(new Course {Active = false, Name = "SWE2", Teacher = teacher1});
            var course3 = ctx.Courses.Add(new Course {Active = true, Name = "FUS", Teacher = teacher2});
            var course4 = ctx.Courses.Add(new Course {Active = true, Name = "VTSE", Teacher = teacher4});
            ctx.SaveChanges();

            var student1 = ctx.Students.Add(new Student
                {BDay = DateTime.Now, FirstName = "ghi1", Name = "jkl1", Class = class1});
            var student2 = ctx.Students.Add(new Student
                {BDay = DateTime.Now, FirstName = "ghi2", Name = "jkl2", Class = class1});
            var student3 = ctx.Students.Add(new Student
                {BDay = DateTime.Now, FirstName = "ghi3", Name = "jkl3", Class = class1});
            var student4 = ctx.Students.Add(new Student
                {BDay = DateTime.Now, FirstName = "ghi4", Name = "jkl4", Class = class1});
            var student5 = ctx.Students.Add(new Student
                {BDay = DateTime.Now, FirstName = "ghi5", Name = "jkl5", Class = class1});

            ctx.SaveChanges();
        }
    }
}