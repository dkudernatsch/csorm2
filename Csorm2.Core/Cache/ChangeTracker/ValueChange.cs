using System;
using Csorm2.Core.Metadata;
using Attribute = Csorm2.Core.Metadata.Attribute;

namespace Csorm2.Core.Cache.ChangeTracker
{
    public interface IValueChange
    {
        object EntityObj { get; }
        object OldValue { get; }
        object NewValue { get; }
        Entity Entity { get; }
        Attribute Attribute { get; }
    }

    public class ValueChange: IValueChange
    {
        public ValueChange(Entity entity, Attribute attribute,  object entityObj, object oldValue, object newValue)
        {
            Entity = entity;
            Attribute = attribute;
            OldValue = oldValue;
            NewValue = newValue;
            EntityObj = entityObj;
        }

        public object EntityObj { get; }
        public Entity Entity { get; }
        public Attribute Attribute { get; }
        public object OldValue { get; }
        public object NewValue { get; }


        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Attribute != null ? Attribute.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OldValue != null ? OldValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (NewValue != null ? NewValue.GetHashCode() : 0);
                return hashCode;
            }
        }

        protected bool Equals(ValueChange other)
        {
            return Equals(Attribute, other.Attribute) && Equals(OldValue, other.OldValue) &&
                   Equals(NewValue, other.NewValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((ValueChange) obj);
        }
    }

    public class DelayedValueChange: IValueChange
    {
        private readonly object _oldEntity;
        private readonly object _newEntity;

        public DelayedValueChange(Entity entity, object oldEntity, object newEntity, Attribute newAttr, Attribute oldAttr)
        {
            Entity = entity;
            _oldEntity = oldEntity;
            _newEntity = newEntity;
            NewAttr = newAttr;
            OldAttr = oldAttr;
        }

        public Entity Entity { get; }
        public Attribute NewAttr { get; }
        public Attribute Attribute => OldAttr;
        public Attribute OldAttr { get; }

        public object EntityObj => _newEntity;
        
        public object OldValue => OldAttr.InvokeGetter(_oldEntity);
        public object NewValue => NewAttr.InvokeGetter(_newEntity);
        
        protected bool Equals(DelayedValueChange other)
        {
            return Equals(_oldEntity, other._oldEntity) && Equals(_newEntity, other._newEntity) && Equals(Entity, other.Entity) && Equals(Attribute, other.Attribute);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() 
                   && Equals((DelayedValueChange) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_oldEntity, _newEntity, Entity, Attribute);
        }
    }
}