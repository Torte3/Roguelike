#nullable enable
using Utilities;
using XNode;

namespace Domain.Model.Dungeon
{
    public interface IMapNodeBlueprint
    {
        Id<MapNode> NodeId { get; }
        int Repeat { get; }
        Node Node { get; }
    }
}
