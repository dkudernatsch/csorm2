using System;
using System.Linq;
using Csorm2.Core.Query;
using Csorm2.Core.Query.Insert;
using Csorm2.Tests.TestClasses;
using Npgsql;
using Xunit;

namespace Csorm2.Tests.Schema
{
    [Collection("IntegrationTest")]
    public class ChangeTrackingTests: IDisposable
    {

        private TestClasses.TestContext _ctx;
        
        public ChangeTrackingTests()
        {
            _ctx = new TestClasses.TestContext(
                () => new NpgsqlConnection("Host=localhost;Port=5432;User Id=user;Password=1234;Database=test_db"));
            _ctx.EnsureRecreated();
        }
        public void Dispose()
        {
            _ctx.EnsureDeleted();
        }


        [Fact]
        public void TrackSimpleChanges()
        {
            
            var student = new Student {Name = "Daniel"};
            
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(student));
            
            student.Name = "Hans";

            var changeList = _ctx.ChangeTracker.CollectChanges();
            Assert.Single(changeList);
            var entityChanges = changeList[_ctx.Schema.EntityTypeMap[typeof(Student)]].ToList();
            Assert.Single(entityChanges);
            var changes = entityChanges[0].Changes;
            Assert.Single(changes);
            var change = changes.First();
            
            Assert.Same(student ,change.EntityObj);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Student)],change.Entity);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Student)].Attributes["Name"], change.Attribute);
            Assert.Equal("Daniel" ,change.OldValue);
            Assert.Equal("Hans" ,change.NewValue);
        }
        
        [Fact]
        public void TrackManyToOneChanges()
        {
            var student = new Student {Name = "Daniel"};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(student));
            
            var grade = new Grade{Id = 1, GradeValue = 2, Student = student};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(grade));
            
            var s = grade.Student;
            grade.Student = null;

            var changeList = _ctx.ChangeTracker.CollectChanges();
            Assert.Single(changeList);
            var entityChanges = changeList[_ctx.Schema.EntityTypeMap[typeof(Grade)]].ToList();
            Assert.Single(entityChanges);
            var changes = entityChanges[0].Changes;
            Assert.Single(changes);
            var change = changes.First();
            
            Assert.Single(changes);

            Assert.Same(grade ,change.EntityObj);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Grade)],change.Entity);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Grade)].Attributes["FK_Grade_Student"], change.Attribute);
            Assert.Equal(s.Id ,change.OldValue);
            Assert.Null(change.NewValue);
        }
        
        [Fact]
        public void TrackManyToOneRemoveChanges()
        {
            var student = new Student {Name = "Daniel"};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(student));
            
            var grade1 = new Grade{Id = 1, GradeValue = 2, Student = student};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(grade1));
            var grade2 = new Grade{Id = 2, GradeValue = 2, Student = student};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(grade2));
            var grade3 = new Grade{Id = 3, GradeValue = 2, Student = student};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(grade3));
            
            student.Grades.Remove(grade2);

            var changeList = _ctx.ChangeTracker.CollectChanges();
            Assert.Single(changeList);
            var entityChanges = changeList[_ctx.Schema.EntityTypeMap[typeof(Grade)]].ToList();
            Assert.Single(entityChanges);
            var changes = entityChanges[0].Changes;
            Assert.Single(changes);
            var change = changes.First();
            
            Assert.Single(changes);

            Assert.Same(grade2 ,change.EntityObj);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Grade)],change.Entity);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Grade)].Attributes["FK_Grade_Student"], change.Attribute);
            Assert.Equal(student.Id ,change.OldValue);
            Assert.Null(change.NewValue);
        }
        
        [Fact]
        public void TrackManyToOneAdditionChanges()
        {
            var student = new Student {Name = "Daniel"};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(student));
            
            var grade1 = new Grade{Id = 1, GradeValue = 2, Student = student};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(grade1));
            var grade2 = new Grade{Id = 2, GradeValue = 2, Student = student};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(grade2));
            var grade3 = new Grade{Id = 3, GradeValue = 2, Student = student};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(grade3));

            var grade4 = new Grade {Id = 4, GradeValue = 5};
            student.Grades.Add(grade4);

            var changeList = _ctx.ChangeTracker.CollectChanges();
            Assert.Single(changeList);
            var entityChanges = changeList[_ctx.Schema.EntityTypeMap[typeof(Grade)]].ToList();
            Assert.Single(entityChanges);
            var changes = entityChanges[0].Changes;
            Assert.Single(changes);
            var change = changes.First();
            
            Assert.Single(changes);

            Assert.Same(grade4 ,change.EntityObj);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Grade)],change.Entity);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Grade)].Attributes["FK_Grade_Student"], change.Attribute);
            
            Assert.Null(change.OldValue);
            Assert.Equal(student.Id ,change.NewValue);
        }
        
        [Fact]
        public void TrackManyToOneMultipleChanges()
        {
            var student = new Student {Name = "Daniel"};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(student));
            
            var grade1 = new Grade{Id = 1, GradeValue = 2, Student = student};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(grade1));
            var grade2 = new Grade{Id = 2, GradeValue = 2, Student = student};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(grade2));
            var grade3 = new Grade{Id = 3, GradeValue = 2, Student = student};
            _ctx.Connection.Insert( new QueryBuilder(_ctx).Insert(grade3));

            var grade4 = new Grade {Id = 4, GradeValue = 5};
            
            student.Grades.Add(grade4);
            student.Grades.Remove(grade1);
            student.Grades.Remove(grade2);
            
            var changeList = _ctx.ChangeTracker.CollectChanges();
            Assert.Single(changeList);
            var entityChanges = changeList[_ctx.Schema.EntityTypeMap[typeof(Grade)]].ToList();
            Assert.Equal(3, entityChanges.Count);
            
            var changes1 = entityChanges[0].Changes;
            Assert.Single(changes1);
            var change1 = changes1.First();
            
            Assert.Single(changes1);

            Assert.Same(grade4 ,change1.EntityObj);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Grade)],change1.Entity);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Grade)].Attributes["FK_Grade_Student"], change1.Attribute);
            
            Assert.Null(change1.OldValue);
            Assert.Equal(student.Id ,change1.NewValue);
            
            
            var changes2 = entityChanges[1].Changes;
            Assert.Single(changes2);
            var change2 = changes2.First();
            
            Assert.Single(changes2);

            Assert.Same(grade1 ,change2.EntityObj);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Grade)],change2.Entity);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Grade)].Attributes["FK_Grade_Student"], change2.Attribute);

            Assert.Equal(student.Id ,change2.OldValue);
            Assert.Null(change2.NewValue);
            
            
            var changes3 = entityChanges[2].Changes;
            Assert.Single(changes3);
            var change3 = changes3.First();
            
            Assert.Single(changes3);

            Assert.Same(grade2 ,change3.EntityObj);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Grade)],change3.Entity);
            Assert.Equal(_ctx.Schema.EntityTypeMap[typeof(Grade)].Attributes["FK_Grade_Student"], change3.Attribute);

            Assert.Equal(student.Id ,change3.OldValue);
            Assert.Null(change3.NewValue);
        }

        [Fact]
        public void TrackManyToManyAdditionChanges()
        {
            var student = new Student {Name = "Daniel"};
            var course = new Course{CourseId = 1, Name = "SWE3", Room = "A3.08"};
            
            _ctx.Connection.Insert(new QueryBuilder(_ctx).Insert(student));
            _ctx.Connection.Insert(new QueryBuilder(_ctx).Insert(course));
            
            var newStudent = new Student{Name = "Viktor"};
            _ctx.Connection.Insert(new QueryBuilder(_ctx).Insert(newStudent));
            
            course.Students.Add(newStudent);

            var changeList = _ctx.ChangeTracker.CollectChanges();
            Assert.Empty(changeList);
            
        }
        
    }
}