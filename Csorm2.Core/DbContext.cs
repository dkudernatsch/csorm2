using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Csorm2.Core.Attributes;
using Csorm2.Core.Cache;
using Csorm2.Core.Cache.ChangeTracker;
using Csorm2.Core.DDL;
using Csorm2.Core.Metadata;
using Csorm2.Core.Metadata.Builders;

namespace Csorm2.Core
{
    public abstract class DbContext
    {
        public DbContext(Func<IDbConnection> connection, bool createDatabase = false)
        {
            OnConfiguring()(Config);
            _connectionProvider = connection;
            InitializeDbSets();
            InitializeSchema();
            ChangeTracker = new ChangeTracker(this);
            if (createDatabase) CreateDatabase();
        }

        public Schema Schema { get; private set; } = new Schema();

        public ObjectCache Cache { get; } = new ObjectCache();

        public DbContextConfiguration Config { get; } = new DbContextConfiguration();

        private readonly Func<IDbConnection> _connectionProvider;
        public DatabaseConnection Connection => new DatabaseConnection(this, _connectionProvider.Invoke());

        public ChangeTracker ChangeTracker { get; }

        //creates generic implementations for each of the users DbSets 
        private void InitializeDbSets()
        {
            var dbSetProperties = GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

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

        public void SaveChanges() => ChangeTracker.SaveChanges();

        private void CreateDatabase()
        {
            var ddl = CreateDdl();
            using var conn = Connection;
            conn.BeginTransaction();
            conn.ExecuteDdl(ddl);
            conn.Commit();
        }
        
        private string CreateDdl()
        {
            var tables = Schema.EntityNameMap.Values.Select(val => new TableDefinition(val));

            var tableDefinitions = tables.ToList();
            
            var constraints = tableDefinitions.SelectMany(t => 
                    t.Constraints()
                        .OrderBy(c => c is PrimaryKeyConstraint)
                        .Select(c => c.AsSqlString()))
                        .Distinct()
                .ToList();
            
            return
                $"{string.Join(";\n", tableDefinitions.Select(t => t.AsSqlString()))};\n\n{string.Join(";\n", constraints)};";
        }
    }
}