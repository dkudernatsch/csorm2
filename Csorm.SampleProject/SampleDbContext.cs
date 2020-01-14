using System;
using System.Data;
using System.Data.Common;
using Csorm.SampleProject.Model;
using Csorm2.Core;

namespace Csorm.SampleProject
{
    public class SampleDbContext: DbContext
    {
        public SampleDbContext(Func<IDbConnection> connection, bool createDb = false) : base(connection, createDb)
        {
        }
        
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Course> Courses { get; set; }
        
        public override Action<DbContextConfiguration> OnConfiguring()
        {
            return (cfg) => { cfg.ConnectionString = "connect"; };
        }
        
    }
}