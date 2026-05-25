#nullable enable
using Domain.Model.Dungeon;
using Utilities;

namespace Game
{
    public record InfiniteSectionCreatedMessage(
        Id<MapNode> NormalSectionId,
        Id<MapNode> BossSectionId,
        FloorSpec NormalFloorSpec,
        FloorSpec BossFloorSpec);
}
