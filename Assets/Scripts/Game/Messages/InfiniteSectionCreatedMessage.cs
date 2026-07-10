#nullable enable
using Domain.Model.Dungeon;
using Domain.Model.Dungeon.Graph;
using Utilities;

namespace Game.Messages
{
    internal record InfiniteSectionCreatedMessage(
        Id<MapNode> NormalSectionId,
        Id<MapNode> BossSectionId,
        FloorSpec NormalFloorSpec,
        FloorSpec BossFloorSpec);
}
