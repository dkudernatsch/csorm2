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
        public SampleDbContext(Func<IDbConnection> connection) 
            : base(connection)
        {
        }
       
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Course> Courses { get; set; }
        
    }
``` 
`connection` should be a function which produces a new IDbConnection every time.

## Code first database creation

CsOrm supports a code first database creation model. By annotating C# pocos you can describe your database layout and csorm supports automatic
generation of the needed DDL. 
CsOrm uses property and class names per default, however `[Table("TableName")"]` and `[Column("ColumnName")]` can be used to adjust naming of the generated database.
 
 #### Entity relations
 
 Csorm supports the following relation types:
 * **1:n** : using the `[OneToMany]` and `[ManyToOne]` attributes *(note that you have to specify all arguments to these attributes, automatic naming of foreign keys and hidden tables is not supported)* 
 * **n:m** using the `[ManyToMany]` attribute
 
 All relation types support 1-way and 2-way definition, meaning you can add the relation to one side of the C# classes or both.
 
 ##### Examples
 
```c#
    //OneToMany relation
    [OneToMany(OtherEntity = typeof(Student), OtherKey = "fk_student_class", ThisKey = "Id")]
    public ICollection<Student> Students;

    // ManyToOne relation
    [ManyToOne(OtherEntity = typeof(Class), OtherKey = "Id", ThisKey = "fk_student_class")]
    public Class Class;


    // ManyToMany relation
    [ManyToMany(
        OtherEntity = typeof(Course),
        RelationTableName = "StudentCourseRel",
        OtherKey = "fK_student_id",
        OtherEntityOtherKey = "fk_course_id",
        ThisKey = "Id",
        OtherEntityThisKey = "OtherNameThanId"
    )]
    public ICollection<Course> Courses;
```
 
 
 ### Lazy Loading
 
Currently CsOrm only supports lazily loading related objects from the database. To accomplish this CsOrm uses a lazyLoader which you need to define in your 
entity classes and acts as an interceptor on property calls.

#### Example
 
```c#
    public class Student
    {
        // needs to be defined to make lazy loading work
        private ILazyLoader Lazy { get; set; } = new LazyLoader();

        private Class _class;

        [ManyToOne(OtherEntity = typeof(Class), OtherKey = "Id", ThisKey = "fk_student_class")]
        public Class Class
        {
            // intercept the getter with the loader, one loaded this will return the inner private  field
            get => Lazy.Load(this, ref _class);
            set => _class = value;
        }
    }
```
 
## Usage

Usage pattern is similar to _EntityFramework Core_: 

Changes to managed objects are automatically tracked and once SaveChanges is called persisted to the database.

### Querying:

Querying can be done directly on DbSets. They support operation to query on entity via primary key:
```c#
    var teacher1 = ctx.Teachers.Find(1L);
```
all entities of a given type:
```c#
    var teachers = ctx.Teachers.All();
```
or via a special WhereQueryFragment:
```c#
    var teachers = ctx.Teachers.Where
        // query all teacher with the name "Teacher1"
        new WhereSqlFragment(
          BinaryExpression.Eq(
              new Accessor {PropertyName = "Name", TableName = "Teacher"}, 
              Value.String("Teacher1", "name")
          )
      ));
```
Note: all operation returning a IEnumerable are lazy and only query the database if they are iterated over.

### Inserts

Inserts are also done using the DbSet instances:

```csharp
    var teacher1 = ctx.Teachers.Add(
        new Teacher{
            BDay = DateTime.Now, 
            FirstName = "abc1", 
            Name = "def1", 
            Salary = 3500});
    ctx.SaveChanges();
```
Note: 
* `DbSet.Add` only informs the changetracker that you wish to persist the given entity. The actual SQL query is only sent if you call `DbContext.SaveChanges` afterward.
* It is not supported to insert entities with untracked objects as their relations, you have to persist them before trying to use them as relation value 
```c#
        var teacher = ctx.Teachers.Add(
            new Teacher{BDay = DateTime.Now, FirstName = "abc4", Name = "def4", Salary = 3500});

        //In order to insert the class object here the teacher must already be a tracked entity
        // without the call to savechanges before the 2nd Add this code will throw an Exception
        ctx.SaveChanges();

        // Insert an object with a ManyToOne relation with existing tracked object
        var class = ctx.Classes.Add(new Class {Name = "BIF1", Teacher = teacher});

        // this call will persist the class with the given teacher as realted object
        ctx.SaveChanges();        
```
### Update

All changes to managed entities are automatically tracked by CsOrm and persisted once `DbContext.SaveChanges` is called. 
 
This includes:

* Simple property changes: 
```csharp
    var teacher = ctx.Teachers.Find(1L);
    var teacher.Salary += 1000; 
    ctx.SaveChanges();
```

* Setting new objects on relations
```c#
    var teacher1 = ctx.Teachers.Find(1L);
    var student = ctx.Students.Find(101L);
    student.Teacher = teacher;
    ctx.SaveChanges();
```

* Adding entities to OneToMany relations

```c#
    var student = ctx.Students.Find(1L);
    var grade = new Grade {Id = 4, GradeValue = 5};
    student.Grades.Add(grade);
```
_Note: also supports updating with a new untracked object, which will be automatically inserted and tracked_

* Removing entities from OneToMany relations

```c#
    var student = ctx.Students.Find(1L);
    student.Grades.Remove(student.Grades[0]);
```

* Adding and removing from ManyToMany relations

```c#
    var student = ctx.Students.Find(1L);
    student.Grades.Remove(student.Grades[0]);
```
