#nullable enable
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using R3;

namespace Game
{
    public interface IReadOnlyWorld
    {
        public IReadOnlyMap? CurrentMap { get; }
        public Observable<OnActiveReadOnlyMapChangedMessage> OnActiveMapChanged { get; }
        public IReadOnlyWorldEventStream Events { get; }
    }
}
