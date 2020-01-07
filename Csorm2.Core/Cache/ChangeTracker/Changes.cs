using System;
using System.Collections;
using System.Collections.Generic;
using Csorm2.Core.Metadata;
using Attribute = Csorm2.Core.Metadata.Attribute;

namespace Csorm2.Core.Cache.ChangeTracker
{
    public class Changes
    {
        public Changes(Entity entity, object obj, object primaryKey)
        {
            Entity = entity;
            this.Obj = obj;
            PrimaryKey = primaryKey;
        }

        private Entity Entity { get; set; }
        public object Obj { get; set; }
        private object PrimaryKey { get; set; }
        private ISet<IValueChange> ValueChanges { get; } = new HashSet<IValueChange>();


        public void AddChange(IValueChange change)
        {
            ValueChanges.Add(change);
        }

        public bool HasChanges()
        {
            return ValueChanges.Count > 0;
        }

        public IEnumerable<IValueChange> ChangesValues()
        {
            return ValueChanges;
        }
        
        public Changes AddChanges(Changes otherChanges)
        {
            if (this.Entity == otherChanges.Entity
                || this.PrimaryKey == otherChanges.PrimaryKey)
                throw new ArgumentException("Changes can only be added to changes of the same entity type and object");

            foreach (var valueChange in otherChanges.ValueChanges)
            {
                if (ValueChanges.Contains(valueChange))
                    throw new ArgumentException("Changes were already tracked by other this change object");

                ValueChanges.Add(valueChange);
            }
            return this;
        }
    }


    public interface IValueChange
    {
        public object OldValue { get; }
        public object NewValue { get; }
        public Entity Entity { get; }
        public Attribute Attribute { get; }
    }
    
    public class ValueChange: IValueChange
    {
        public ValueChange(Entity entity, Attribute attribute, object oldValue, object newValue)
        {
            Entity = entity;
            Attribute = attribute;
            OldValue = oldValue;
            NewValue = newValue;
        }

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
        public object OldValue => OldAttr.PropertyInfo.GetMethod.Invoke(_oldEntity, new object[] { });
        public object NewValue => NewAttr.PropertyInfo.GetMethod.Invoke(_newEntity, new object[] { });


        protected bool Equals(DelayedValueChange other)
        {
            return Equals(_oldEntity, other._oldEntity) && Equals(_newEntity, other._newEntity) && Equals(Entity, other.Entity) && Equals(Attribute, other.Attribute);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DelayedValueChange) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_oldEntity, _newEntity, Entity, Attribute);
        }
    }
}