#nullable enable
using R3;

namespace Domain.Model.WorldEvents
{
    public interface IReadOnlyWorldEventStream
    {
        public Observable<WorldEvent> OnRecorded { get; }
    }
}
