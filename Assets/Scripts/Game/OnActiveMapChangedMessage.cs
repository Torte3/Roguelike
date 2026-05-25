#nullable enable
namespace Game
{
    public record OnActiveMapChangedMessage(MapManager Map, MapManager? PreviousMap, bool IsNewWorld);
}