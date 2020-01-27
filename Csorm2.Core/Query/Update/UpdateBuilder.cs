using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Csorm2.Core.Cache.ChangeTracker;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query.Expression;
using Attribute = Csorm2.Core.Metadata.Attribute;

namespace Csorm2.Core.Query.Update
{
    public class UpdateBuilder
    {
        private DbContext _context;

        public UpdateBuilder(DbContext context)
        {
            _context = context;
        }

        public UpdateSetBuilder<TEntity> Update<TEntity>()
        {
            var entity = _context.Schema.EntityTypeMap[typeof(TEntity)];
            return new UpdateSetBuilder<TEntity>(_context, entity);
        }

        public UpdateSetBuilder<object> Update(Entity entity)
        {
            return new UpdateSetBuilder<object>(_context, entity);
        }
    }

   public class UpdateSetBuilder<TEntity>
    {
        private DbContext _context;
        private Entity _entity;

        public UpdateSetBuilder(DbContext context, Entity entity)
        {
            _context = context;
            _entity = entity;
        }

        public UpdateStatement<TEntity> SetValues(TEntity obj, IEnumerable<IValueChange> valueChanges)
        {
            return new UpdateStatement<TEntity>(_context, _entity, obj, valueChanges);
        }
    }

    public class UpdateStatement<TEntity> : ISqlExpression
    {
        private DbContext _context;
        public Entity Entity { get; }
        private TEntity _object;
        private IEnumerable<IValueChange> _changes;

        private IEnumerable<Attribute> _returningAttributes;

        private WhereSqlFragment _whereClause;

        public UpdateStatement(DbContext context, Entity entity, TEntity obj, IEnumerable<IValueChange> changes)
        {
            _object = obj;
            _context = context;
            Entity = entity;
            _changes = changes;
            _returningAttributes = Entity.Attributes.Values.Where(attr => !attr.IsEntityType);
            _whereClause = new WhereSqlFragment(BinaryExpression.Eq(
                new Accessor {TableName = Entity.TableName, PropertyName = Entity.PrimaryKeyAttribute.DataBaseColumn},
                Value.FromAttr(
                    Entity.PrimaryKeyAttribute.InvokeGetter(_object),
                    Entity.PrimaryKeyAttribute
                )));
        }

        public string AsSqlString()
        {
           var update = _changes.Aggregate("",
                (acc, change) => acc == ""
                    ? $"{change.Attribute.DataBaseColumn} = @{change.Attribute.DataBaseColumn}0"
                    : acc + $", {change.Attribute.DataBaseColumn} = @{change.Attribute.DataBaseColumn}0");
            var table = $"UPDATE {Entity.TableName} ";
            var set = $"SET {update}";
            var returningAttrs =
                _returningAttributes.Aggregate("", (acc, attr) => acc == "" ? attr.DataBaseColumn : acc + ", " + attr.DataBaseColumn);
            var returning = $"RETURNING {returningAttrs}";
            var where = $"WHERE {_whereClause.AsSqlString()}";
            return $"{table} " +
                   $"{set}" +
                   $" {where}" +
                   $" {returning}";
        }

        public IEnumerable<(DbType, string, object)> GetParameters()
        {
            return _changes.Select((c) =>
            {
                Debug.Assert(c.Attribute.DatabaseType != null, "c.Attribute.DatabaseType != null");
                return (c.Attribute.DatabaseType.Value, c.Attribute.DataBaseColumn + '0', c.NewValue);
            }).Concat(_whereClause.GetParameters());
        }
    }
}
