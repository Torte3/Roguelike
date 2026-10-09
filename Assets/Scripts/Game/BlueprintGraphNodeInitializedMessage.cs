#nullable enable
using Domain.Model.Dungeon;
using Utilities;

namespace Game
{
    internal record BlueprintGraphNodeInitializedMessage(Id<MapNode> MapNodeId);
}
