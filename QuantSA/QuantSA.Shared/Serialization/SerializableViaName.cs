using System;

namespace QuantSA.Shared.Serialization
{
    public abstract class SerializableViaName : ISerializableViaName, IEquatable<ISerializableViaName>
    {
        public bool Equals(ISerializableViaName other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != GetType()) return false;
            return string.Equals(GetName(), other.GetName());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ISerializableViaName) obj);
        }

        public static bool operator ==(SerializableViaName left, SerializableViaName right)
        {
            if ((object) left == null && (object) right == null) return true;
            if ((object) left != null && (object) right == null) return false;
            if ((object) left == null && (object) right != null) return false;
            return left.GetHashCode() == right.GetHashCode();
        }

        public static bool operator !=(SerializableViaName left, SerializableViaName right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return GetName() != null ? GetName().GetHashCode() : 0;
        }

        public abstract string GetName();
    }
}