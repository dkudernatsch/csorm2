using System;
using System.Data;
using System.Data.Common;
using Csorm2.Core;

namespace Csorm2.Tests.TestClasses
{
    public class TestContext: DbContext
    {
        public TestContext(Func<IDbConnection> connection, bool createDb = false) : base(connection, createDb)
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