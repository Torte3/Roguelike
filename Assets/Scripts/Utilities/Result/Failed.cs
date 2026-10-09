#nullable enable
namespace Utilities.Result
{
    public sealed class Failed<T, TFailure> : Result<T, TFailure>
    {
        private readonly TFailure _failure;

        public Failed(TFailure failure)
        {
            _failure = failure;
        }

        public override bool IsOk(out T value)
        {
            value = default!;
            return false;
        }

        public override bool IsFailed(out TFailure failure)
        {
            failure = _failure;
            return true;
        }
    }
}
