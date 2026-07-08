#nullable enable
using Game.Map;

namespace Game.Messages
{
    internal record OnActiveMapChangedMessage(MapManager Map);
}