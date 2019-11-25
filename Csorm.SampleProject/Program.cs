using System;
using System.Security.Claims;
using Csorm.SampleProject.Model;
using Csorm2.Core.Schema.Builders;

namespace Csorm.SampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new SchemaBuilder(new[]
            {
                typeof(Class),
                typeof(Course),
                typeof(Person),
                typeof(Student),
                typeof(Teacher),
            });
            builder.Build();
            Console.WriteLine();
        }
    }
}