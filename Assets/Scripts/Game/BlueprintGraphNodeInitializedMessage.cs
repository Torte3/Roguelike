#nullable enable
using Domain.Model.Dungeon;
using Utilities;

namespace Game
{
    public record BlueprintGraphNodeInitializedMessage(Id<MapNode> MapNodeId);
}
