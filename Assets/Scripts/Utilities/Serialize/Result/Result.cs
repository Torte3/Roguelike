#nullable enable
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Utilities.Serialize.Result
{
    public static class Result
    {
        public static Result<T> Error<T>()
        {
            return new Error<T>();
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Ok<T>(value);
        }

        public static Result<T> ToResult<T>(this T? value) where T : struct
        {
            if (value == null)
            {
                return Result<T>.Error;
            }

            return new Ok<T>(value.Value);
        }

        public static Result<T> ToResult<T>(this T? value) where T : class
        {
            if (value == null)
            {
                return Result<T>.Error;
            }

            return new Ok<T>(value);
        }
    }

    [Serializable]
    public class Result<T> : IEquatable<Result<T>>
    {
        public static Result<T> Error => new Error<T>();
        public bool IsError => !isOk;
        public bool IsOk() => isOk;
        public bool IsOk(out T value)
        {
            value = this.value;
            return isOk;
        }

        [SerializeField] private bool isOk;
        [SerializeReference] private T value;

        public T? Value => UnwrapOrNull();

        public Result()
        {
            isOk = false;
            value = default;
        }

        public Result(T value)
        {
            isOk = true;
            this.value = value;
        }

        public bool Equals(Result<T> other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (isOk != other.isOk)
                return false;

            return !isOk || Equals(value, other.value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Result<T>)
                return Equals((Result<T>)obj);
            return false;
        }

        private string FriendlyName(Type t)
        {
            return t.FullName;
        }

        public T Expect(string msg)
        {
            return IsOk() ? value : throw new Exception(msg);
        }

        public T Unwrap()
        {
            return IsOk() ? value : throw new Exception($"Tried to unwrap a Error<{FriendlyName(typeof(T))}>!");
        }

        public T UnwrapOr(T def = default)
        {
            return IsOk() ? value : def;
        }

        public T UnwrapOr(Func<T> provider)
        {
            return IsOk() ? value : provider();
        }

        public T? UnwrapOrNull()
        {
            return IsOk() ? value : default;
        }

        public Result<U> Map<U>(Func<T, U> converter)
        {
            return IsOk() ? new Result<U>(converter(value)) : new Error<U>();
        }

        public async Task<Result<U>> Map<U>(Func<T, Task<U>> converter)
        {
            return IsOk() ? new Result<U>(await converter(value)) : new Error<U>();
        }

        public U MapOr<U>(U def, Func<T, U> converter)
        {
            return IsOk() ? converter(value) : def;
        }

        public U MapOr<U>(Func<U> provider, Func<T, U> converter)
        {
            return IsOk() ? converter(value) : provider();
        }

        public void Match(Action<T> onOk, Action onError)
        {
            if (IsOk())
                onOk(value);
            else
                onError();
        }

        public Result<U> And<U>(Result<U> option)
        {
            return IsError ? Result<U>.Error : option;
        }

        public Result<U> AndThen<U>(Func<T, Result<U>> option)
        {
            return IsError ? Result<U>.Error : option(value);
        }

        public Task<Result<U>> AndThen<U>(Func<T, Task<Result<U>>> option)
        {
            return IsError ? Task.FromResult(Result<U>.Error) : option(value);
        }

        public Result<T> Or(Result<T> other)
        {
            return IsOk() ? this : other;
        }

        public Result<T> OrElse(Func<Result<T>> option)
        {
            return IsOk() ? this : option();
        }

        public Task<Result<T>> OrElse(Func<Task<Result<T>>> option)
        {
            return IsOk() ? Task.FromResult(this) : option();
        }

        public void Take()
        {
            value = default;
            isOk = false;
        }

        public override int GetHashCode()
        {
            return !isOk ? 0 : ReferenceEquals(value, null) ? -1 : value.GetHashCode();
        }

        public static bool operator ==(Result<T> left, Result<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Result<T> left, Result<T> right)
        {
            return !left.Equals(right);
        }
    }
}