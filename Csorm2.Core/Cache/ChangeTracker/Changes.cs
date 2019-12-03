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
            this.obj = obj;
            PrimaryKey = primaryKey;
        }

        private Entity Entity { get; set; }
        private object obj { get; set; }
        private object PrimaryKey { get; set; }
        private ISet<ValueChange> ValueChanges { get; } = new HashSet<ValueChange>();


        public void AddChange(ValueChange change)
        {
            ValueChanges.Add(change);
        }

        public bool HasChanges()
        {
            return ValueChanges.Count > 0;
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

    public class ValueChange
    {
        public ValueChange(Attribute attribute, object oldValue, object newValue)
        {
            Attribute = attribute;
            OldValue = oldValue;
            NewValue = newValue;
        }

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
}