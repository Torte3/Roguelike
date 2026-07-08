#nullable enable
namespace Utilities.Result
{
    public sealed class Ok<T, TFailure> : Result<T, TFailure>
    {
        private readonly T _value;

        public Ok(T value)
        {
            _value = value;
        }

        public override bool IsOk(out T value)
        {
            value = _value;
            return true;
        }
    }
}
