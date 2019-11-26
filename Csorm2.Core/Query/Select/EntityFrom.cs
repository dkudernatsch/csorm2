using Csorm2.Core.Metadata;

namespace Csorm2.Core.Query.Select
{
    public class EntityFrom: IFrom
    {
        private Entity _inner;
        private string _alias;
        
        public EntityFrom(Entity inner, string alias = null)
        {
            _inner = inner;
            _alias = alias;
        }

        public bool HasAlias()
        {
            return true;
        }

        public string GetAlias()
        {
            return _alias ?? _inner.TableName;
        }

        public string AsFromFragment()
        {
            return $"{_inner.TableName}";
        }
    }
}