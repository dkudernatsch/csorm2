using System.Collections.Generic;
using System.Linq;
using Csorm2.Core.Query;
using Csorm2.Core.Query.Expression;
using BinaryExpression = Csorm2.Core.Query.Expression.BinaryExpression;

namespace Csorm2.Core
{
    /// <summary>
    /// Object representing an interface to the database based on Enity types
    /// </summary>
    /// <typeparam name="T">type of the entity</typeparam>
    public abstract class DbSet<T>
    {
        private DbContext _ctx;
        protected DbSet(DbContext ctx)
        {
            _ctx = ctx;
        }
        /// <summary>
        /// Gets All entities for a specific type
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> All()
        {
            var query = new QueryBuilder(_ctx)
                .Select<T>().Build();
            return _ctx.Connection.Select(query);
        }
        /// <summary>
        /// Queries a subset of all entities of type T
        /// </summary>
        /// <param name="where">Query filter</param>
        /// <returns></returns>
        public IEnumerable<T> Where(WhereSqlFragment where)
        {
            var query = new QueryBuilder(_ctx)
                .Select<T>()
                .Where(where)
                .Build();
            return _ctx.Connection.Select(query);
        }
        /// <summary>
        /// Finds a specific entity with the given primary key
        /// </summary>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Adds an untracked entity to the database
        /// note this operation only adds this change to a tracked list
        /// use Context.SaveChanges to persist as insert
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T Add(T entity)
        {
            return _ctx.ChangeTracker.InsertNew(entity);
        }
        /// <summary>
        /// removes a tracked entity from the database
        /// note this operation only adds this change to a tracked list
        /// use Context.SaveChanges to persist as delete
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public void Remove(T entity)
        {
            _ctx.ChangeTracker.Delete(entity);
        }

    }
}
