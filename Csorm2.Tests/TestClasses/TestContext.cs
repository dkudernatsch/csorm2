using System;
using System.Data.Common;
using Csorm2.Core;

namespace Csorm2.Tests.TestClasses
{
    public class TestContext: DbContext
    {
        public TestContext(DbConnection connection) : base(connection)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Course> Courses { get; set; }
        
        public override Action<DbContextConfiguration> OnConfiguring()
        {
            return (cfg) => { cfg.ConnectionString = ""; };
        }
    }
}