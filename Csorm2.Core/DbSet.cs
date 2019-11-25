namespace Csorm2.Core
{
    public abstract class DbSet<T>
    {

        private DbContext _ctx;


        protected DbSet(DbContext ctx)
        {
            _ctx = ctx;
        }
    }
}