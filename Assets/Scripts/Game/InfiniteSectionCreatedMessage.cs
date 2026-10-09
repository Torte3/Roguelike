#nullable enable
using Domain.Model.Dungeon;
using Utilities;

namespace Game
{
    internal record InfiniteSectionCreatedMessage(
        Id<MapNode> NormalSectionId,
        Id<MapNode> BossSectionId,
        FloorSpec NormalFloorSpec,
        FloorSpec BossFloorSpec);
}
