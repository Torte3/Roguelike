#nullable enable
namespace Utilities.Result
{
    public abstract class Result<T, TFailure>
    {
        public static Result<T, TFailure> Ok(T value)
        {
            return new Ok<T, TFailure>(value);
        }

        public static Result<T, TFailure> Failed(TFailure failure)
        {
            return new Failed<T, TFailure>(failure);
        }

        public static Result<T, TFailure> Cancelled()
        {
            return new Cancelled<T, TFailure>();
        }

        public abstract bool IsOk(out T value);

        public virtual bool IsFailed(out TFailure failure)
        {
            failure = default!;
            return false;
        }

        public virtual bool IsCancelled => false;
    }
}
