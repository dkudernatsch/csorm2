using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query.Expression;
using Attribute = Csorm2.Core.Metadata.Attribute;

namespace Csorm2.Core.Query.Insert
{
    public class InsertQueryBuilder
    {
        private DbContext _context;

        public InsertQueryBuilder(DbContext context)
        {
            _context = context;
        }

        public Insert<T> Insert<T>()
        {
            var entity = _context.Schema.EntityTypeMap[typeof(T)];
            return new Insert<T>(_context, entity);
        }
        
        public Insert<object> Insert(Entity e)
        {
            return new Insert<object>(_context, e);
        }
    }

    public class Insert<T>
    {
        private DbContext _ctx;
        private Entity _entity;

        public Insert(DbContext ctx, Entity entity)
        {
            _entity = entity;
            _ctx = ctx;
        }

        public InsertStatement<T> Value(T obj)
        {
            return Values(new[] {obj});
        }

        public InsertStatement<T> Values(IEnumerable<T> obj)
        {
            return new InsertStatement<T>(obj, _ctx, _entity);
        }
    }

    public class InsertStatement<T> : IStatement<T>
    {
        private IEnumerable<T> _toInsert;
        private DbContext _context;
        public Entity Entity { get; }

        public InsertStatement(IEnumerable<T> toInsert, DbContext context, Entity entity)
        {
            _toInsert = toInsert;
            _context = context;
            Entity = entity;
        }

        private IEnumerable<Attribute> InsertAbleAttributes => Entity.Attributes.Values
            .Where(attr => !attr.IsEntityType)
            .Where(attr => !attr.IsAutoInc);

        private IEnumerable<Attribute> AutoIncAttributes => Entity.Attributes.Values
            .Where(attr => attr.IsAutoInc);
        
        public string AsSqlString()
        {
            var toInsert = InsertAbleAttributes.Select(attr => attr.DataBaseColumn)
                .Aggregate("", (acc, next) => acc == "" ? next : acc + ", " + next);
            
            var insertPositions = InsertAbleAttributes.Aggregate("", (acc, next) => acc == "" ? $"@{next.DataBaseColumn}?" : acc + ", " + $"@{next.DataBaseColumn}?");
            
            var insertPlaceHolder = _toInsert.Select((s, i) => (i, s)).Aggregate("",
                (acc, next) => acc == "" ? $"({insertPositions.Replace("?", ""+next.i)})" : acc + $", ({insertPositions.Replace("?", ""+next.i)})");
        
            var autoIncReturn = AutoIncAttributes
                .Select(attr => attr.DataBaseColumn)
                .Aggregate("", (acc, next) => acc == "" ? next : acc + ", " + next);
            
            return $"INSERT INTO {Entity.TableName} ({toInsert}) " +
                   $"VALUES {insertPlaceHolder} " +
                   (string.IsNullOrWhiteSpace(autoIncReturn) ? "" : $"Returning {autoIncReturn}");
        }

        public IEnumerable<(DbType, string, object)> GetParameters()
        {
            Func<Attribute, object, object> extractSimpleValue = (attr, obj) => attr.InvokeGetter(obj);
            Func<Attribute, object, object> extractForeignKey = (attr, obj) =>
            {
                var relation = Entity.Attributes.FirstOrDefault(relattr => relattr.Value?.Relation?.FromKeyAttribute == attr).Value?.Relation;
                
                if (!(relation is ManyToOne manyToOne))
                    throw new Exception("Tried to insert entity attribute that corresponds to other table");
                
                var otherEntity = relation.ToEntity;
                var otherObj = relation.FromEntityAttribute.InvokeGetter(obj);
                var otherPk = otherObj == null ? null :
                    otherEntity.PrimaryKeyAttribute.InvokeGetter(otherObj);
                
                if (otherObj != null && (otherPk == null || _context.Cache.ObjectPool[otherEntity].GetValueOrDefault(otherPk) == null))
                {
                    throw new Exception("Trying to insert entity with unmanaged object relation value");
                }
                return otherPk;
            };
            
            return _toInsert.SelectMany((obj, i) => InsertAbleAttributes.Select(attr =>
                (attr.DatabaseType.Value, 
                    attr.DataBaseColumn + i, 
                    attr.IsShadowAttribute ? extractForeignKey(attr, obj) : extractSimpleValue(attr, obj))));
        }

        public IEnumerable<IEnumerable<(Attribute, T)>> ReturnValuePositions
            => _toInsert.Select(obj => AutoIncAttributes.Select(attr => (attr, obj)));
    }
}