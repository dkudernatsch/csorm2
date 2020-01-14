# CSOrm

CSOrm is a toy Object Relational Mapper developed for a university project. _It is not meant to be used either in production or even in personal projects._
CSOrm is highly unstable and is just as likely to nuke your database as save something correctly. Only use this code if you want to research how to implement various orm features badly.

## features

* persist your c# objects to a database (only postgres is guaranteed to sometimes work)
* generate your database form c# objects
* Query, Update, and Delete objects and persist changes to the database


## Getting started

CsOrm is inspired by Entity Framework Core and its concept of a DbContext should be very familiar to people who have used it.
To start with CSorm you first have to extend your own DbContext and add DbSet properties for your various entity types.DbContext

```csharp
    public class SampleDbContext: DbContext
    {
        public SampleDbContext(Func<IDbConnection> connection, bool createDb = false) 
            : base(connection, createDb)
        {
        }
       
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Course> Courses { get; set; }
        
    }
``` 
connection should be a function which produces a new IDbConnection every time and via createDB you can generate 
and execute DDL the first time the application is run. 

Usage is similar to ef core: 

Changes to managed objects are automatically tracked and once SaveChanges is called persisted to the database.

Query:
Inserts: 
```csharp
    using var ctx = ctx = new SampleDbContext(CONNECTION, true);
    var teacher1 = ctx.Teachers.Find(1L);
```

Inserts: 
```csharp
    using var ctx = ctx = new SampleDbContext(CONNECTION, true);
    var teacher1 = ctx.Teachers.Add(new Teacher{BDay = DateTime.Now, FirstName = "abc1", Name = "def1", Salary = 3500});
    ctx.SaveChanges();
```

Update: 
```csharp
    using var ctx = ctx = new SampleDbContext(CONNECTION, true);
    var teacher1 = ctx.Teachers.Find(1L);
    var teacher1.Salary += 1000; 
    ctx.SaveChanges();
```
