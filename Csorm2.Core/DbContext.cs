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
    /// <summary>
    /// DbContext represents the session with a database and can be used to insert, update, delete and query objects as entities from and to the database.
    /// Each DbContext holds an internal Cache of queried objects to ensure object identity, when querying the database, however identity is not preserved across multiple contexts
    /// 
    /// </summary>
    public abstract class DbContext : IDisposable

    {
        /// <summary>
        /// Initializes a new DbContext with the given Connection provider
        /// The connection provider is invoked each time a sql query will be sent
        /// Theoretically supports any <see cref="IDbConnection"/>, however, only the postgres connector is tested
        /// </summary>
        /// <param name="connection"></param>
        public DbContext(Func<IDbConnection> connection)
        {
            _connectionProvider = connection;
            InitializeDbSets();
            InitializeSchema();
            ChangeTracker = new ChangeTracker(this);
            Cache = new ObjectCache(this);
        }
        /// <summary>
        /// Provides access to the schema of the current database context
        /// Any Entities <typeparam name="{T}"/> provieded as <see cref="DbSet{T}"/> will be converted to an in memeory representation of the database schema (table, attributes, relations, ...)
        /// </summary>
        public Schema Schema { get; private set; } = new Schema();
        /// <summary>
        /// Primary Object cache 
        /// </summary>
        public ObjectCache Cache { get; }
        

        private readonly Func<IDbConnection> _connectionProvider;
        /// <summary>
        /// Invokes Connection Provider
        /// </summary>
        public DatabaseConnection Connection => new DatabaseConnection(this, _connectionProvider.Invoke());
        
        /// <summary>
        /// Provides access to information and operations for entity instances this context is tracking.
        /// </summary>
        public ChangeTracker ChangeTracker { get; }

        //creates generic implementations for each of the users DbSets 
        private void InitializeDbSets()
        {
            // fetch all properties of this object of type DbSet<?>
            var dbSetProperties = GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));
            
            // for each dbset initialize a generic implementation
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
        /// <summary>
        /// Builds the database schema from DbSets of this instance
        /// </summary>
        private void InitializeSchema()
        {
            // get entity types from dbsets 
            // any dbset<T> -> collect T as entity type
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
        /// <summary>
        /// Persist all changes made to tracked objects since the last time SaveChanges has been called
        /// </summary>
        public void SaveChanges()
        {
            ChangeTracker.SaveChanges();
        }
        /// <summary>
        /// Ensures that all Database tables have been dropped and recreated
        /// Use this method onyl if you dont care about the data currently in the database and you want new empty tables
        /// </summary>
        public void EnsureRecreated()
        {
            EnsureDeleted();
            EnsureCreated();
        }
        /// <summary>
        /// Ensures that all database tables exist
        /// will not recreate tables that are already in the database 
        /// </summary>
        public void EnsureCreated()
        {
            var ddl = CreateDdl();
            using var conn = Connection;
            conn.BeginTransaction();
            conn.ExecuteDdl(ddl);
            conn.Commit();
        }
        /// <summary>
        /// Drops all tables and associated data from the database
        /// All data will be lost
        /// </summary>
        public void EnsureDeleted()
        {
            var ddl = RemoveDdl();
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

        private string RemoveDdl()
        {
            var tables = Schema.EntityNameMap.Values.Select(val => val.TableName).Select(table =>
                $"DROP TABLE IF EXISTS {table} CASCADE;\n");
            return string.Join("", tables);
        }
        /// <summary>
        /// Disposes of the current DbContext instance
        /// </summary>
        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}