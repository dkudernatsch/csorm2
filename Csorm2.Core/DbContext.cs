using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Csorm2.Core.Attributes;
using Csorm2.Core.Cache;
using Csorm2.Core.Metadata;
using Csorm2.Core.Metadata.Builders;

namespace Csorm2.Core
{
 public abstract class DbContext
    {
        public DbContext(DbConnection connection)
        {
            OnConfiguring()(Config);
            InitializeDbSets();
            InitializeSchema();
            Connection = new DatabaseConnection(this, connection);
        }
        
        public Schema Schema { get; private set; } = new Schema();
        
        public ObjectCache Cache { get; } = new ObjectCache();
        
        public DbContextConfiguration Config { get; } = new DbContextConfiguration();

        public DatabaseConnection Connection { get; }

        //creates generic implementations for each of the users DbSets
        private void InitializeDbSets()
        {
            var dbSetProperties = GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

            foreach (var dbSetProperty in dbSetProperties) 
            {
                if (dbSetProperty.CanRead && dbSetProperty.CanWrite)
                {
                    var genericArgument = dbSetProperty.PropertyType.GetGenericArguments()[0];
                    var dbSet = DbSetProvider.ProvideDbSet(genericArgument, this);
                    dbSetProperty.SetMethod?.Invoke(this, new[] {dbSet});
                }
            }
        }

        private void InitializeSchema()
        {
            var entityTypes = GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => p.PropertyType.GetGenericArguments()[0]);

            var schemaBuilder = new SchemaBuilder(entityTypes);
            schemaBuilder.Build();
            foreach (var entity in schemaBuilder.Context.Entities)
            {
                Schema.AddEntity(entity.Value);
            }
            
        }

        public abstract Action<DbContextConfiguration> OnConfiguring();

    }
}