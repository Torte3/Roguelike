#nullable enable
using R3;

namespace Domain.Model.WorldEvents
{
    public class WorldEventStream : IWorldEventRecorder, IReadOnlyWorldEventStream
    {
        private readonly Subject<WorldEvent> _onRecorded = new();
        private readonly Subject<Unit> _onCleared = new();

        public Observable<WorldEvent> OnRecorded => _onRecorded;
        public Observable<Unit> OnCleared => _onCleared;

        public void Record(WorldEvent worldEvent)
        {
            _onRecorded.OnNext(worldEvent);
        }

        public void AnnounceCleared()
        {
            _onCleared.OnNext(Unit.Default);
        }
    }
}
