using System;
using System.Security.Claims;
using Csorm.SampleProject.Model;
using Csorm2.Core.Metadata.Builders;
using Csorm2.Core.Query;
using Csorm2.Core.Query.Select;

namespace Csorm.SampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var ctx = new SampleDbContext(null);

            var squery = new QueryBuilder(ctx).Select<Student>().Build();
            var cquery = new QueryBuilder(ctx).Select<Teacher>().Build();
            var tquery = new QueryBuilder(ctx).Select<Course>().Build();
            var clquery = new QueryBuilder(ctx).Select<Class>().Build();
            
            Console.WriteLine(squery.AsSqlString());
            Console.WriteLine(cquery.AsSqlString());
            Console.WriteLine(tquery.AsSqlString());
            Console.WriteLine(clquery.AsSqlString());
        }
    }
}