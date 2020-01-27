using System;
using System.Collections.Generic;
using System.Data;
using Csorm2.Core.Metadata.Builders;
using Csorm2.Tests.TestClasses;
using Xunit;

namespace Csorm2.Tests.Schema
{
    public class SchemaBuilderTests
    {
        
        [Fact]
        public void TestEntityExists()
        {
            var schemaBuilder = new SchemaBuilder(new []
            {
                typeof(Course),
                typeof(Student),
                typeof(Grade),
            });
            schemaBuilder.Build();
            var schema = new Core.Metadata.Schema();
            foreach (var entity in schemaBuilder.Context.Entities.Values)
            {
                schema.AddEntity(entity);
            }
            
            Assert.NotNull(schema.EntityTypeMap[typeof(Student)]);
            Assert.NotNull(schema.EntityTypeMap[typeof(Course)]);
            Assert.NotNull(schema.EntityTypeMap[typeof(Grade)]);
        }
        
        [Fact]
        public void TestEntitiesHaveAttributes()
        {
            var schemaBuilder = new SchemaBuilder(new []
            {
                typeof(Course),
                typeof(Student),
                typeof(Grade),
            });
            schemaBuilder.Build();
            var schema = new Core.Metadata.Schema();
            foreach (var e in schemaBuilder.Context.Entities.Values)
            {
                schema.AddEntity(e);
            }

            var entity = schema.EntityTypeMap[typeof(Student)];
            Assert.NotEmpty(entity.Attributes);
            
            var gradeAttr = entity.Attributes.GetValueOrDefault("Grades");
            Assert.NotNull(gradeAttr);
            Assert.Equal("Grades", gradeAttr.Name);
            Assert.NotNull(gradeAttr.Relation);
            Assert.True(gradeAttr.IsEntityType);
            Assert.False(gradeAttr.IsAutoInc);
            Assert.False(gradeAttr.IsShadowAttribute);
            
            var idAttr = entity.Attributes.GetValueOrDefault("Id");
            Assert.NotNull(idAttr);
            Assert.True(idAttr.IsAutoInc);
            Assert.Equal(DbType.Int64, idAttr.DatabaseType);
            Assert.Equal(typeof(long?), idAttr.ClrType);
            Assert.Null(idAttr.Relation);
            Assert.False(idAttr.IsShadowAttribute);

            var nameAttr = entity.Attributes.GetValueOrDefault("Name");
            Assert.NotNull(nameAttr);
            Assert.False(nameAttr.IsAutoInc);
            Assert.Equal(DbType.String, nameAttr.DatabaseType);
            Assert.Equal(typeof(string), nameAttr.ClrType);
            Assert.False(nameAttr.IsShadowAttribute);


        }
        
        
    }
}