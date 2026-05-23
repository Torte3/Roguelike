#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using R3;
using Domain.Model.Dungeon;
using Domain.Model.Map;
using Domain.Model.Memento;
using Utilities;

namespace Game
{
    public class Dungeon : ISerializable<DungeonMemento>
    {
        private readonly DungeonTopology _topology;
        private readonly IDungeonBlueprintData _blueprint;
        private readonly int _maxFiniteDepth;
        private readonly Dictionary<Id<MapNode>, FloorSpec> _floorSpecByMapNode;

        private Id<IMap>? _startMapId;

        public Id<IMap> StartMapId
        {
            get
            {
                if (_startMapId == null)
                    InitializeNewGame();
                return _startMapId!;
            }
        }

        public Dungeon(DungeonMemento memento)
        {
            _blueprint = ObjectLoader.Load<DungeonBluePrintData>("Dungeon");
            _maxFiniteDepth = _blueprint.GetMaxFiniteDepth();
            _floorSpecByMapNode = new Dictionary<Id<MapNode>, FloorSpec>();
            _topology = new DungeonTopology(
                _blueprint,
                new Dictionary<Id<IMap>, Id<MapNode>>(memento.MapNodeByInstance),
                new Dictionary<Id<IMap>, int>(memento.DepthByInstance),
                memento.SectionsByInfiniteNode,
                memento.BossSectionsByInfiniteNode);
            SubscribeTopologyEvents();

            RestoreFloorSpecsFromMemento(memento);
            _topology.RestorePredecessorsBySection();

            var startIds = _topology.GetMapIds(_blueprint.GetStartMapNodeId());
            _startMapId = startIds.Count > 0 ? startIds[0] : null;
        }

        public Dungeon(DungeonBluePrintData blueprint)
        {
            _blueprint = blueprint;
            _maxFiniteDepth = _blueprint.GetMaxFiniteDepth();
            _floorSpecByMapNode = new Dictionary<Id<MapNode>, FloorSpec>();
            _topology = new DungeonTopology(_blueprint);
            SubscribeTopologyEvents();
        }

        public void InitializeNewGame()
        {
            _startMapId = _topology.GetMapIds(_blueprint.GetStartMapNodeId())[0];
        }

        public int GetDepth(Id<IMap> mapId) => _topology.GetDepth(mapId);

        public float GetProgress(Id<IMap> mapId)
        {
            if (_maxFiniteDepth <= 0) return 1f;

            var depth = GetDepth(mapId);
            return depth <= _maxFiniteDepth ? depth / (float)_maxFiniteDepth : 1f;
        }

        public FloorSpec GetFloorSpec(Id<IMap> mapId) =>
            _floorSpecByMapNode[_topology.GetMapNodeId(mapId)];

        public List<MapConnection> GetDestinations(Id<IMap> mapId) =>
            _topology.GetDestinations(mapId);

        public bool ShouldBatchCreateSection(Id<IMap> mapId)
        {
            if (!_topology.TryGetMapNodeId(mapId, out var ownerId)) return false;
            if (!_topology.IsInfiniteSection(ownerId)) return false;
            var ids = _topology.GetMapIds(ownerId);
            return ids[0] == mapId;
        }

        public IReadOnlyList<Id<IMap>> GetSectionMapIds(Id<IMap> sectionHeadMapId)
        {
            var normalSectionId = _topology.GetMapNodeId(sectionHeadMapId);
            var bossSectionId = _topology.GetBossSectionId(normalSectionId);
            var mapIds = new List<Id<IMap>>(_topology.GetMapIds(normalSectionId));
            mapIds.AddRange(_topology.GetMapIds(bossSectionId));
            return mapIds;
        }

        public MapMemento CreateMapManager(Id<IMap> id, IEnumerable<MovementData> movementData)
        {
            var spec = GetFloorSpec(id);
            var progress = GetProgress(id);
            if (spec.Field == null)
            {
                var mapBuilder = new WorldMapBuilder(id, "seed");
                foreach (var data in movementData)
                    mapBuilder.AddMovementEntity(data);
                return mapBuilder.Build();
            }

            var mapBuilder2 = new MapBuilder(spec.Field, spec.WaterChance, spec, progress, id);
            foreach (var data in movementData)
                mapBuilder2.AddMovementEntity(data);
            return mapBuilder2.Build();
        }

        public DungeonMemento Serialize() =>
            DungeonMemento.Create(
                _topology.MapNodeByMapId,
                _topology.DepthByMapId,
                _topology.SectionsByInfiniteNode,
                _topology.BossSectionsByInfiniteNode,
                _floorSpecByMapNode.Where(p =>
                    _topology.IsInfiniteSection(p.Key) || _topology.IsInfiniteBossSection(p.Key)));

        private void SubscribeTopologyEvents()
        {
            _topology.OnInfiniteSectionCreated.Subscribe(OnInfiniteSectionCreated);
            _topology.OnBlueprintGraphNodeInitialized.Subscribe(OnBlueprintGraphNodeInitialized);
        }

        private void OnBlueprintGraphNodeInitialized(BlueprintGraphNodeInitializedMessage message) =>
            _floorSpecByMapNode.TryAdd(message.MapNodeId, _blueprint.CreateFloorSpec(message.MapNodeId));

        private void OnInfiniteSectionCreated(InfiniteSectionCreatedMessage message)
        {
            _floorSpecByMapNode[message.NormalSectionId] = message.NormalFloorSpec;
            _floorSpecByMapNode[message.BossSectionId] = message.BossFloorSpec;
        }

        private void RestoreFloorSpecsFromMemento(DungeonMemento memento)
        {
            var blueprint = (DungeonBluePrintData)_blueprint;

            foreach (var entry in memento.FloorSpecByMapNode)
            {
                var mapNodeId = entry.MapNodeId;
                if (!_topology.TryGetInfiniteGraphId(mapNodeId, out var infGraphId)) continue;

                var floorData = _topology.IsInfiniteBossSection(mapNodeId)
                    ? _blueprint.GetInfiniteBossFloorData(infGraphId)
                    : _blueprint.GetInfiniteFloorData(infGraphId);
                _floorSpecByMapNode[mapNodeId] = FloorSpec.Build(entry.FloorSpec, floorData, blueprint);
            }

            foreach (var mapNodeId in _topology.AllMapNodeIds)
            {
                if (_floorSpecByMapNode.ContainsKey(mapNodeId)) continue;

                if (_blueprint.IsGraphMapNode(mapNodeId))
                    _floorSpecByMapNode[mapNodeId] = _blueprint.CreateFloorSpec(mapNodeId);
            }
        }
    }
}
