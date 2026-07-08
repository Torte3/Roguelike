#nullable enable
namespace Game
{
    internal record OnActiveMapChangedMessage(MapManager Map, bool IsNewWorld);
}