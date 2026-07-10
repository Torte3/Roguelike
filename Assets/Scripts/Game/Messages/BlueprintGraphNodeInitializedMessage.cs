#nullable enable
using Domain.Model.Dungeon;
using Domain.Model.Dungeon.Graph;
using Utilities;

namespace Game.Messages
{
    internal record BlueprintGraphNodeInitializedMessage(Id<MapNode> MapNodeId);
}
