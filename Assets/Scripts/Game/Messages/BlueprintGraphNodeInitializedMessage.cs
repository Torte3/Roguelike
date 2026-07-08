#nullable enable
using Domain.Model.Dungeon;
using Domain.Model.Dungeon.Graph;
using Utilities;

namespace Game.Messages
{
    public record BlueprintGraphNodeInitializedMessage(Id<MapNode> MapNodeId);
}
