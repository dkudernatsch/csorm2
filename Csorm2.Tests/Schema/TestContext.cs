using System;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Csorm2.Core;
using Csorm2.Core.Query;
using Csorm2.Core.Query.Expression;
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
            var postgres =
                new NpgsqlConnection("Host=10.10.1.1;Port=54321;User Id=csorm;Password=csorm;Database=csorm");
            var ctx = new TestClasses.TestContext(postgres);
            var db = ctx.Connection;
            var students1 = db.Select(new QueryBuilder(ctx)
                .Select<Student>()
                .Build());

            var s1 = students1.First().Grades.First().Student.Grades.First();
            
            
            
            //Assert.NotEqual(s1, s2);

           
            _testOutputHelper.WriteLine(JsonConvert.SerializeObject(s1));
            //_testOutputHelper.WriteLine(JsonConvert.SerializeObject(s2));
            
        }
    }
}