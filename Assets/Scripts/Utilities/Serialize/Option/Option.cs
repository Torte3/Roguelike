#nullable enable
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Utilities.Serialize.Option
{
    public static class Option
    {
        public static Option<T> None<T>()
        {
            return new None<T>();
        }

        public static Option<T> Some<T>(T value)
        {
            return new Some<T>(value);
        }

        public static Option<T> ToOption<T>(this T? value) where T : struct
        {
            if (value == null)
            {
                return Option<T>.None;
            }

            return new Some<T>(value.Value);
        }

        public static Option<T> ToOption<T>(this T? value) where T : class
        {
            if (value == null)
            {
                return Option<T>.None;
            }

            return new Some<T>(value);
        }
    }

    [Serializable]
    public class Option<T> : IEquatable<Option<T>>
    {
        public static Option<T> None => new None<T>();
        public bool IsNone => !hasValue;
        public bool IsSome() => hasValue;
        public bool IsSome(out T value)
        {
            value = this.value;
            return hasValue;
        }
        public bool HasValue => hasValue;

        [SerializeField] private bool hasValue;
        [SerializeReference] private T value;

        public T? Value => UnwrapOrNull();

        public Option()
        {
            hasValue = false;
            value = default;
        }

        public Option(T? value)
        {
            hasValue = value != null;
            if (hasValue)
                this.value = value!;
        }

        public bool Equals(Option<T> other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (hasValue != other.hasValue)
                return false;

            return !hasValue || Equals(value, other.value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Option<T>)
                return Equals((Option<T>)obj);
            return false;
        }

        private string FriendlyName(Type t)
        {
            return t.FullName;
        }

        public T Expect(string msg)
        {
            return IsSome(out value) ? value : throw new Exception(msg);
        }

        public T Unwrap()
        {
            return IsSome() ? value : throw new Exception($"Tried to unwrap a None<{FriendlyName(typeof(T))}>!");
        }

        public T UnwrapOr(T def = default)
        {
            return IsSome() ? value : def;
        }

        public T UnwrapOr(Func<T> provider)
        {
            return IsSome() ? value : provider();
        }

        public T? UnwrapOrNull()
        {
            return IsSome() ? value : default;
        }

        public Option<U> Map<U>(Func<T, U> converter)
        {
            return IsSome() ? new Option<U>(converter(value)) : new None<U>();
        }

        public async Task<Option<U>> Map<U>(Func<T, Task<U>> converter)
        {
            return IsSome() ? new Option<U>(await converter(value)) : new None<U>();
        }

        public U MapOr<U>(U def, Func<T, U> converter)
        {
            return IsSome() ? converter(value) : def;
        }

        public U MapOr<U>(Func<U> provider, Func<T, U> converter)
        {
            return IsSome() ? converter(value) : provider();
        }

        public Option<U> And<U>(Option<U> option)
        {
            return IsNone ? Option<U>.None : option;
        }

        public Option<U> AndThen<U>(Func<T, Option<U>> option)
        {
            return IsNone ? Option<U>.None : option(value);
        }

        public Task<Option<U>> AndThen<U>(Func<T, Task<Option<U>>> option)
        {
            return IsNone ? Task.FromResult(Option<U>.None) : option(value);
        }

        public Option<T> Or(Option<T> other)
        {
            return IsSome() ? this : other;
        }

        public Option<T> OrElse(Func<Option<T>> option)
        {
            return IsSome() ? this : option();
        }

        public Task<Option<T>> OrElse(Func<Task<Option<T>>> option)
        {
            return IsSome() ? Task.FromResult(this) : option();
        }

        public void Take()
        {
            value = default;
            hasValue = false;
        }

        public override int GetHashCode()
        {
            return !hasValue ? 0 : ReferenceEquals(value, null) ? -1 : value.GetHashCode();
        }

        public static bool operator ==(Option<T> left, Option<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Option<T> left, Option<T> right)
        {
            return !left.Equals(right);
        }
    }
}