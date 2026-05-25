#nullable enable
using System;
using UnityEngine;

namespace Utilities
{
    [Serializable]
    public class Id<T> : IEquatable<Id<T>>
    {
        public static Id<T> Empty => new(Guid.Empty);
        [SerializeField] private readonly string _value;
        public Guid Value => Guid.Parse(_value);

        public Id(Guid value)
        {
            _value = value.ToString();
        }

        public Id(string value)
        {
            _ = Guid.Parse(value);
            _value = value;
        }

        public static Id<T> Generate()
        {
            return new Id<T>(Guid.NewGuid());
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static bool operator ==(Id<T>? a, Id<T>? b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Value == b.Value;
        }

        public static bool operator !=(Id<T>? a, Id<T>? b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            return obj is Id<T> id && Equals(id);
        }

        public bool Equals(Id<T> other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}