using System.Collections.Generic;
using System.Linq;
using Csorm2.Core.Metadata;
using Csorm2.Core.Query.Expression;

namespace Csorm2.Core.Query.Select
{
    public class SelectQueryBuilder
    {
        private readonly DbContext _ctx;

        public SelectQueryBuilder(DbContext ctx)
        {
            _ctx = ctx;
        }

        public SelectFromQueryBuilder<TEntity> FromTable<TEntity>(string table)
        {
            var from = new EntityFrom(_ctx.Schema.EntityNameMap.Values.First(e => e.TableName == table));
            
            return new SelectFromQueryBuilder<TEntity>(_ctx, from);
        }
        
        
    }

    public class SelectFromQueryBuilder<TEntity>
    {
        private readonly IFrom _from;
        private readonly DbContext _ctx;

        public SelectFromQueryBuilder(DbContext ctx, IFrom from)
        {
            _from = from;
            _ctx = ctx;
        }

        public SelectFromQueryBuilder<TEntity> Join(string otherTable, string myKey, string otherKey)
        {
            var entity = _ctx.Schema.EntityTypeMap[typeof(TEntity)];
            var myKeyAttr = entity.Attributes.Values.First(attr => attr.DataBaseColumn == myKey);
            
            var otherEntity = _ctx.Schema.EntityTypeMap.Values.First(e => e.TableName == otherTable);
            var otherKeyAttr = otherEntity.Attributes.Values.First(attr => attr.DataBaseColumn == otherKey);
            
            var join = new InnerJoin(_from ?? new EntityFrom(entity), myKeyAttr, new EntityFrom(otherEntity), otherKeyAttr);
            
            return new SelectFromQueryBuilder<TEntity>(_ctx, join);
        }
        
        public SelectFromQueryBuilder<TEntity> Join(Entity otherTable, Attribute myKey, Attribute otherKey)
        {
            var entity = _ctx.Schema.EntityTypeMap[typeof(TEntity)];
            var join = new InnerJoin(_from ?? new EntityFrom(entity), myKey, new EntityFrom(otherTable), otherKey);
            return  new SelectFromQueryBuilder<TEntity>(_ctx, join);
        }
        
        public SelectFromWhereQueryBuilder<TEntity> Select(IEnumerable<string> fields)
        {
            return new SelectFromWhereQueryBuilder<TEntity>(_ctx, _from, fields);
        }
        
    }

    public class SelectFromWhereQueryBuilder<TEntity>
    {
        
        private readonly IFrom _from;
        private IEnumerable<string> _fields;
        private readonly IList<WhereSqlFragment> _wheres = new List<WhereSqlFragment>();
        private readonly DbContext _ctx;

        public SelectFromWhereQueryBuilder(DbContext ctx, IFrom from, IEnumerable<string> fields)
        {
            _from = from;
            _fields = fields;
            _ctx = ctx;
        }

        public SelectFromWhereQueryBuilder<TEntity> Where(WhereSqlFragment filter)
        {
            _wheres.Add(filter);
            return this;
        }

        public SelectQuery<TEntity> Build()
        {
            var entity = _ctx.Schema.EntityTypeMap[typeof(TEntity)];
            return new SelectQuery<TEntity>( _from, entity.Attributes.Values.Where(attr => !attr.IsEntityType), _wheres);
        }
    }
}