#nullable enable
using Domain.Model.Map;
using Domain.Model.WorldEvents;

namespace Game.Map
{
    internal sealed class PlayerUnderfootRecorder
    {
        private readonly IMap _map;
        private PlayerUnderfootChanged? _recorded;

        public PlayerUnderfootRecorder(IMap map)
        {
            _map = map;
        }

        public void RecordChanges()
        {
            var underfoot = new PlayerUnderfootChanged(_map.PlayerUnderfoot());
            if (underfoot == _recorded)
                return;

            _recorded = underfoot;
            _map.Events.Record(underfoot);
        }
    }
}
