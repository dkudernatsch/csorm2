namespace Csorm2.Core
{
    public class InternalDbSet<T>: DbSet<T>
    {
        public InternalDbSet(DbContext ctx) 
            : base(ctx)
        {
        }
    }
}