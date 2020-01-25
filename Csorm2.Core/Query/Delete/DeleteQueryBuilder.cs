using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query.Expression;

namespace Csorm2.Core.Query.Delete
{
    public class DeleteQueryBuilder
    {
        private DbContext _ctx;

        public DeleteQueryBuilder(DbContext ctx)
        {
            _ctx = ctx;
        }

        public DeleteQuery<T> Delete<T>(T obj)
        {
            var entity = _ctx.Schema.EntityTypeMap[typeof(T)];
            return new DeleteQuery<T>(_ctx, entity, obj);
        }

        public DeleteQuery<T> Delete<T>(T obj, Entity entity)
        {
            return new DeleteQuery<T>(_ctx, entity, obj);
        }

    }

    public class DeleteQuery<T>: IStatement<T>
    {
        private DbContext _ctx;
        public Entity Entity { get; }
        private T obj;
        private WhereSqlFragment _where;

        public DeleteQuery(DbContext ctx, Entity entity, T obj)
        {
            _ctx = ctx;
            Entity = entity;
            this.obj = obj;
            var pk = Entity.PrimaryKeyAttribute.InvokeGetter(obj);

            _where = new WhereSqlFragment(
                BinaryExpression.Eq(
                    new Accessor{TableName = Entity.TableName, PropertyName = Entity.PrimaryKeyAttribute.DataBaseColumn},
                    Value.FromAttr(pk, Entity.PrimaryKeyAttribute)
                    ));
        }

        public string AsSqlString()
        {
            var del = $"DELETE FROM {Entity.TableName} ";
            return $"{del} WHERE {_where.AsSqlString()}";
        }

        public IEnumerable<(DbType, string, object)> GetParameters()
        {
            return _where.GetParameters();
        }

        public IEnumerable<IEnumerable<(Attribute, T)>> ReturnValuePositions => new IEnumerable<(Attribute, T)>[0];
    }
}
