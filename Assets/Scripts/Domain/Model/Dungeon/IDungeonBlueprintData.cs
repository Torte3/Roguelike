#nullable enable
using System.Collections.Generic;
using Domain.Model.Memento;
using RandomDungeonWithBluePrint;
using Utilities;

namespace Domain.Model.Dungeon
{
    public interface IDungeonBlueprintData
    {
        string Name { get; }
        Id<MapNode> GetStartMapNodeId();
        IEnumerable<Id<MapNode>> GetNextMapNodeIds(Id<MapNode> graphMapNodeId);
        IEnumerable<Id<MapNode>> GetPrevMapNodeIds(Id<MapNode> graphMapNodeId);
        IEnumerable<Id<MapNode>> GetTeleportInMapNodeIds(Id<MapNode> graphMapNodeId);
        IEnumerable<Id<MapNode>> GetTeleportOutMapNodeIds(Id<MapNode> graphMapNodeId);
        FloorSpec CreateFloorSpec(Id<MapNode> graphMapNodeId);
        (FloorSpec Normal, FloorSpec Boss) PickRandomInfiniteSectionFloorSpecs(Id<MapNode> infiniteGraphNodeId);
        FloorData GetInfiniteFloorData(Id<MapNode> infiniteGraphNodeId);

        FloorData GetInfiniteBossFloorData(Id<MapNode> infiniteGraphNodeId);
        bool IsInfiniteTemplate(Id<MapNode> graphMapNodeId);
        bool IsGraphMapNode(Id<MapNode> mapNodeId);
        int GetRepeat(Id<MapNode> graphMapNodeId);
        int GetMaxFiniteDepth();
        IReadOnlyDictionary<Id<MapNode>, int> GetDepthByGraphNode();
        IEnumerable<Id<MapNode>> GetAllGraphMapNodeIds();
    }
}
