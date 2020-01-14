using System;
using Csorm2.Core.DDL;
using Csorm2.Tests.TestClasses;
using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace Csorm2.Tests.Ddl
{
    public class TestTableDefinition
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestTableDefinition(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TestTable()
        {
            var ctx =  new TestContext(
                () => new NpgsqlConnection("Host=localhost;Port=5432;User Id=user;Password=1234;Database=test_db"), true);
            
            var tabledef = new TableDefinition(ctx.Schema.EntityTypeMap[typeof(Student)]);
            
            
        }
    }
}