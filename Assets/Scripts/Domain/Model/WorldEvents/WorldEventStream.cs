#nullable enable
using R3;

namespace Domain.Model.WorldEvents
{
    public class WorldEventStream : IWorldEventRecorder, IReadOnlyWorldEventStream
    {
        private readonly Subject<WorldEvent> _onRecorded = new();

        public Observable<WorldEvent> OnRecorded => _onRecorded;

        public void Record(WorldEvent worldEvent)
        {
            _onRecorded.OnNext(worldEvent);
        }
    }
}
