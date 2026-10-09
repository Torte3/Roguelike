#nullable enable
namespace Utilities.Result
{
    public sealed class Cancelled<T, TFailure> : Result<T, TFailure>
    {
        public override bool IsOk(out T value)
        {
            value = default!;
            return false;
        }

        public override bool IsCancelled => true;
    }
}
