using System.Collections.Generic;
using System.Linq;
using Csorm2.Core.Query;
using Csorm2.Core.Query.Expression;
using BinaryExpression = Csorm2.Core.Query.Expression.BinaryExpression;

namespace Csorm2.Core
{
    public abstract class DbSet<T>
    {
        private DbContext _ctx;
        protected DbSet(DbContext ctx)
        {
            _ctx = ctx;
        }

        public IEnumerable<T> All()
        {
            var query = new QueryBuilder(_ctx)
                .Select<T>().Build();
            return _ctx.Connection.Select(query);
        }

        public IEnumerable<T> Where(WhereSqlFragment where)
        {
            var query = new QueryBuilder(_ctx)
                .Select<T>()
                .Where(where)
                .Build();
            return _ctx.Connection.Select(query);
        }

        public T Find(object primaryKey)
        {
            var entity = _ctx.Schema.EntityTypeMap[typeof(T)];
            var pkAttr = entity.PrimaryKeyAttribute;

            var filter = new WhereSqlFragment(
                BinaryExpression.Eq(
                    new Accessor
                    {
                        PropertyName = pkAttr.DataBaseColumn,
                        TableName = entity.TableName
                    }, Value.FromAttr(primaryKey, pkAttr)));

            var query = new QueryBuilder(_ctx)
                .Select<T>()
                .Where(filter)
                .Build();

            return _ctx.Connection.Select(query).FirstOrDefault();
        }

        public T Add(T entity)
        {
            return _ctx.ChangeTracker.InsertNew(entity);
        }

        public void Remove(T entity)
        {
            _ctx.ChangeTracker.Delete(entity);
        }

    }
}
