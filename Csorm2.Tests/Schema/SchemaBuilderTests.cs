using System;
using Csorm2.Core.Schema.Builders;
using Csorm2.Tests.TestClasses;
using Xunit;

namespace Csorm2.Tests.Schema
{
    public class SchemaBuilderTests
    {
        
        [Fact]
        public void TestSchema()
        {
            var builder = new SchemaBuilder(new []{typeof(A), typeof(B)});
            builder.Build();
            Console.WriteLine();
        }
        
        
    }
}