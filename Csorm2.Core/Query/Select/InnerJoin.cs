using Csorm2.Core.Metadata;

namespace Csorm2.Core.Query.Select
{
    public class InnerJoin : IFrom
    {
        public InnerJoin(IFrom thisEntity, Attribute thisJoinAttr, IFrom joinedEntity, Attribute joinedAttr,
            string alias = null)
        {
            _thisEntity = thisEntity;
            _thisJoinAttr = thisJoinAttr;
            _joinedEntity = joinedEntity;
            _joinedAttr = joinedAttr;
            _alias = alias;
        }

        private IFrom _thisEntity;
        private Attribute _thisJoinAttr;

        private IFrom _joinedEntity;
        private Attribute _joinedAttr;

        private string _alias;

        public string GetAlias()
        {
            return _alias;
        }

        public bool HasAlias()
        {
            return _alias != null;
        }

        public string AsFromFragment()
        {
            var thisEntity = _thisEntity.AsFromFragment();
            var thisAlias = _thisEntity.HasAlias() ? _thisEntity.GetAlias() : null;

            var joinedEntity = _joinedEntity.AsFromFragment();
            var joinedAlias = _joinedEntity.HasAlias() ? _joinedEntity.GetAlias() : null;


            return $"({thisEntity}{(_thisEntity.HasAlias() ? $" as {thisAlias}" : "")}" +
                   " INNER JOIN " +
                   $"{joinedEntity} {(_joinedEntity.HasAlias() ? $" as {joinedAlias}" : "")}" +
                   " ON " +
                   $"{(_thisEntity.HasAlias() ? $"{thisAlias}." : "")}{_thisJoinAttr.DataBaseColumn} " +
                   " = " +
                   $"{(_joinedEntity.HasAlias() ? $"{joinedAlias}." : "")}{_joinedAttr.DataBaseColumn})" +
                   $"{(HasAlias() ? $" as {GetAlias()}" : "")}";
        }
    }
}