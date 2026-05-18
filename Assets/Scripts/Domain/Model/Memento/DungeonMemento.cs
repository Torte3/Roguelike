#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Dungeon;
using Domain.Model.Map;
using UnityEngine;
using Utilities;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class DungeonMemento
    {
        [SerializeField] private SerializableDictionary<string, string> _mapNodeByInstance;
        public Dictionary<Id<IMap>, Id<MapNode>> MapNodeByInstance =>
            _mapNodeByInstance.ToDictionary(
                pair => new Id<IMap>(pair.Key),
                pair => new Id<MapNode>(pair.Value));

        [SerializeField] private SerializableDictionary<string, int> _depthByInstance;
        public Dictionary<Id<IMap>, int> DepthByInstance =>
            _depthByInstance.ToDictionary(
                pair => new Id<IMap>(pair.Key),
                pair => pair.Value);

        [field: SerializeField] public List<SectionsEntry> SectionsByInfiniteNode { get; private set; }
        [field: SerializeField] public List<FloorSpecByMapNodeEntry> FloorSpecByMapNode { get; private set; }

        public static DungeonMemento Create(
            IReadOnlyDictionary<Id<IMap>, Id<MapNode>> mapNodeByInstance,
            IReadOnlyDictionary<Id<IMap>, int> depthByInstance,
            IReadOnlyDictionary<Id<MapNode>, List<Id<MapNode>>> sectionsByInfiniteNode,
            IEnumerable<KeyValuePair<Id<MapNode>, FloorSpec>> infiniteFloorSpecs)
        {
            return new DungeonMemento(
                mapNodeByInstance,
                depthByInstance,
                sectionsByInfiniteNode.Select(p => new SectionsEntry(p.Key, p.Value)).ToList(),
                infiniteFloorSpecs
                    .Select(p => new FloorSpecByMapNodeEntry(p.Key, p.Value.Serialize()))
                    .ToList());
        }

        private DungeonMemento(
            IReadOnlyDictionary<Id<IMap>, Id<MapNode>> mapNodeByInstance,
            IReadOnlyDictionary<Id<IMap>, int> depthByInstance,
            List<SectionsEntry> sectionsByInfiniteNode,
            List<FloorSpecByMapNodeEntry> floorSpecByMapNode)
        {
            _mapNodeByInstance = mapNodeByInstance
                .ToDictionary(pair => pair.Key.ToString(), pair => pair.Value.ToString())
                .ToSerializable();
            _depthByInstance = depthByInstance
                .ToDictionary(pair => pair.Key.ToString(), pair => pair.Value)
                .ToSerializable();
            SectionsByInfiniteNode = sectionsByInfiniteNode;
            FloorSpecByMapNode = floorSpecByMapNode;
        }

        public static DungeonMemento Empty() => Create(
            new Dictionary<Id<IMap>, Id<MapNode>>(),
            new Dictionary<Id<IMap>, int>(),
            new Dictionary<Id<MapNode>, List<Id<MapNode>>>(),
            Array.Empty<KeyValuePair<Id<MapNode>, FloorSpec>>());

        [Serializable]
        public class SectionsEntry
        {
            [SerializeField] private string _infiniteGraphNodeId;
            public Id<MapNode> InfiniteGraphNodeId => new(_infiniteGraphNodeId);

            [SerializeField] private List<string> _sectionIds;
            public List<Id<MapNode>> SectionIds =>
                _sectionIds.Select(id => new Id<MapNode>(id)).ToList();

            public SectionsEntry(Id<MapNode> infiniteGraphNodeId, List<Id<MapNode>> sectionIds)
            {
                _infiniteGraphNodeId = infiniteGraphNodeId.ToString();
                _sectionIds = sectionIds.Select(id => id.ToString()).ToList();
            }
        }

        [Serializable]
        public class FloorSpecByMapNodeEntry
        {
            [SerializeField] private string _mapNodeId;
            public Id<MapNode> MapNodeId => new(_mapNodeId);

            [field: SerializeField] public FloorSpecMemento FloorSpec { get; private set; }

            public FloorSpecByMapNodeEntry(Id<MapNode> mapNodeId, FloorSpecMemento floorSpec)
            {
                _mapNodeId = mapNodeId.ToString();
                FloorSpec = floorSpec;
            }
        }
    }
}
