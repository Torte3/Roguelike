#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Dungeon;
using Domain.Model.Map;
using Domain.Model.Memento;
using UnityEngine;
using Utilities;

namespace Game
{
    public class Dungeon : ISerializable<DungeonMemento>
    {
        private readonly IDungeonBlueprintData _blueprint;
        private readonly int _maxFiniteDepth;

        private readonly Dictionary<Id<MapNode>, FloorSpec> _floorSpecByMapNode;
        private readonly Dictionary<Id<IMap>, Id<MapNode>> _mapNodeByInstance;
        private readonly Dictionary<Id<IMap>, int> _depthByInstance;
        private readonly Dictionary<Id<MapNode>, List<Id<MapNode>>> _sectionsByInfiniteNode;
        private readonly Dictionary<Id<MapNode>, Id<MapNode>> _predecessorBySection = new();

        private Id<IMap>? _startMapId;

        public Id<IMap> StartMapId
        {
            get
            {
                if (_startMapId == null)
                {
                    InitializeNewGame();
                }
                return _startMapId!;
            }
        }

        public Dungeon(DungeonMemento memento)
        {
            _blueprint = ObjectLoader.Load<DungeonBluePrintData>("Dungeon");
            _maxFiniteDepth = _blueprint.GetMaxFiniteDepth();
            _mapNodeByInstance = new Dictionary<Id<IMap>, Id<MapNode>>(memento.MapNodeByInstance);
            _depthByInstance = new Dictionary<Id<IMap>, int>(memento.DepthByInstance);
            _sectionsByInfiniteNode = memento.SectionsByInfiniteNode.ToDictionary(
                entry => entry.InfiniteGraphNodeId,
                entry => entry.SectionIds.ToList());
            _floorSpecByMapNode = new Dictionary<Id<MapNode>, FloorSpec>();
            var blueprint = (DungeonBluePrintData)_blueprint;
            foreach (var entry in memento.FloorSpecByMapNode)
            {
                var mapNodeId = entry.MapNodeId;
                if (!TryGetInfiniteGraphId(mapNodeId, out var infGraphId)) continue;

                var floorData = _blueprint.GetInfiniteFloorData(infGraphId);
                _floorSpecByMapNode[mapNodeId] = FloorSpec.Build(entry.FloorSpec, floorData, blueprint);
            }

            var mapNodeIds = new HashSet<Id<MapNode>>(_mapNodeByInstance.Values);
            foreach (var mapNodeId in mapNodeIds)
            {
                if (_floorSpecByMapNode.ContainsKey(mapNodeId)) continue;

                if (_blueprint.IsGraphMapNode(mapNodeId))
                    _floorSpecByMapNode[mapNodeId] = _blueprint.CreateFloorSpec(mapNodeId);
            }

            _predecessorBySection.Clear();
            var depthByGraph = _blueprint.GetDepthByGraphNode();
            foreach (var (infiniteGraphNodeId, sectionIds) in _sectionsByInfiniteNode)
            {
                for (var i = 0; i < sectionIds.Count; i++)
                {
                    var sectionId = sectionIds[i];
                    if (i > 0)
                    {
                        _predecessorBySection[sectionId] = sectionIds[i - 1];
                        continue;
                    }

                    var instances = GetInstanceIds(sectionId);
                    if (instances.Count == 0) continue;

                    var expectedPredLast = _depthByInstance[instances[0]] - 1;
                    var predecessor = depthByGraph.Keys
                        .Where(graphId => CanReachInfiniteTemplate(graphId, infiniteGraphNodeId))
                        .Where(graphId => GetDepthLast(graphId) == expectedPredLast)
                        .OrderByDescending(graphId => GetInstanceIds(graphId).Count > 0)
                        .ThenBy(graphId => depthByGraph[graphId])
                        .FirstOrDefault();
                    if (predecessor != null)
                        _predecessorBySection[sectionId] = predecessor;
                }
            }

            var startNodeId = _blueprint.GetStartMapNodeId();
            var startIds = GetInstanceIds(startNodeId);
            _startMapId = startIds.Count > 0 ? startIds[0] : null;
        }

        public Dungeon(DungeonBluePrintData blueprint)
        {
            _blueprint = blueprint;
            _maxFiniteDepth = _blueprint.GetMaxFiniteDepth();
            _mapNodeByInstance = new Dictionary<Id<IMap>, Id<MapNode>>();
            _depthByInstance = new Dictionary<Id<IMap>, int>();
            _sectionsByInfiniteNode = new Dictionary<Id<MapNode>, List<Id<MapNode>>>();
            _floorSpecByMapNode = new Dictionary<Id<MapNode>, FloorSpec>();
        }

        public static DungeonMemento Build() => DungeonMemento.Empty();

        public void InitializeNewGame()
        {
            var startNodeId = _blueprint.GetStartMapNodeId();
            EnsureGraphNodeRegistered(startNodeId);
            _startMapId = GetInstanceIds(startNodeId)[0];
        }

        public int GetDepth(Id<IMap> instanceId) => _depthByInstance[instanceId];

        public float GetProgress(Id<IMap> instanceId)
        {
            if (_maxFiniteDepth <= 0) return 1f;

            var depth = GetDepth(instanceId);
            return depth <= _maxFiniteDepth ? depth / (float)_maxFiniteDepth : 1f;
        }

        public FloorSpec GetFloorSpec(Id<IMap> instanceId)
        {
            var nodeId = _mapNodeByInstance[instanceId];
            var spec = _floorSpecByMapNode[nodeId];
            if (!TryGetInfiniteGraphId(nodeId, out var infiniteGraphId))
                return spec;

            var ids = GetInstanceIds(nodeId);
            if (ids[^1] != instanceId)
                return spec;

            return _blueprint.ApplyInfiniteSectionBossFloor(spec, infiniteGraphId);
        }

        public List<MapConnection> GetDestinations(Id<IMap> instanceId)
        {
            var ownerId = _mapNodeByInstance[instanceId];
            var ids = GetInstanceIds(ownerId);
            var index = ids.IndexOf(instanceId);
            var connections = new List<MapConnection>();

            if (index > 0)
            {
                connections.Add(new MapConnection(MovementEntityType.UpStairs, ids[index - 1]));
            }

            if (index == 0)
            {
                var addedPrevUpStairs = false;
                if (IsInfiniteSection(ownerId)
                    && _predecessorBySection.TryGetValue(ownerId, out var predecessorId))
                {
                    var predecessorIds = GetInstanceIds(predecessorId);
                    if (predecessorIds.Count > 0)
                    {
                        connections.Add(new MapConnection(
                            MovementEntityType.UpStairs, predecessorIds[^1]));
                        addedPrevUpStairs = true;
                    }
                }

                if (!addedPrevUpStairs)
                {
                    var graphNodeId = ResolveGraphNodeId(ownerId);
                    foreach (var prevGraphId in _blueprint.GetPrevMapNodeIds(graphNodeId))
                    {
                        EnsureGraphNodeRegistered(prevGraphId);
                        var prevIds = GetInstanceIds(prevGraphId);
                        connections.Add(new MapConnection(MovementEntityType.UpStairs, prevIds[^1]));
                    }
                }
            }

            if (index < ids.Count - 1)
            {
                connections.Add(new MapConnection(MovementEntityType.DownStairs, ids[index + 1]));
                AddTeleportConnections(connections, ownerId, instanceId);
                return connections;
            }

            if (IsInfiniteSection(ownerId))
            {
                var infGraphId = GetInfiniteGraphId(ownerId);
                connections.Add(new MapConnection(MovementEntityType.DownStairs,
                    ReserveNextSection(ownerId, infGraphId)));
            }
            else
            {
                foreach (var nextGraphId in _blueprint.GetNextMapNodeIds(ownerId))
                {
                    if (_blueprint.IsInfiniteTemplate(nextGraphId))
                    {
                        connections.Add(new MapConnection(MovementEntityType.DownStairs,
                            ReserveNextSection(ownerId, nextGraphId)));
                    }
                    else
                    {
                        EnsureGraphNodeRegistered(nextGraphId);
                        connections.Add(new MapConnection(MovementEntityType.DownStairs,
                            GetInstanceIds(nextGraphId)[0]));
                    }
                }
            }

            AddTeleportConnections(connections, ownerId, instanceId);
            return connections;
        }

        private void AddTeleportConnections(List<MapConnection> connections, Id<MapNode> ownerId, Id<IMap> instanceId)
        {
            var ids = GetInstanceIds(ownerId);
            var graphNodeId = ResolveGraphNodeId(ownerId);

            if (ids[0] == instanceId)
            {
                foreach (var teleportIn in _blueprint.GetTeleportInMapNodeIds(graphNodeId))
                {
                    EnsureGraphNodeRegistered(teleportIn);
                    var targetIds = GetInstanceIds(teleportIn);
                    connections.Add(new MapConnection(MovementEntityType.MagicCircle, targetIds[^1]));
                }
            }

            if (ids[^1] == instanceId)
            {
                foreach (var teleportOut in _blueprint.GetTeleportOutMapNodeIds(graphNodeId))
                {
                    EnsureGraphNodeRegistered(teleportOut);
                    connections.Add(new MapConnection(MovementEntityType.MagicCircle,
                        GetInstanceIds(teleportOut)[0]));
                }
            }
        }

        private Id<MapNode> ResolveGraphNodeId(Id<MapNode> ownerId) =>
            TryGetInfiniteGraphId(ownerId, out var infiniteGraphId) ? infiniteGraphId : ownerId;

        public bool ShouldBatchCreateSection(Id<IMap> instanceId)
        {
            if (!_mapNodeByInstance.TryGetValue(instanceId, out var ownerId)) return false;
            if (!IsInfiniteSection(ownerId)) return false;
            var ids = GetInstanceIds(ownerId);
            return ids[0] == instanceId;
        }

        public IReadOnlyList<Id<IMap>> GetSectionInstanceIds(Id<IMap> sectionHeadInstanceId)
        {
            var sectionId = _mapNodeByInstance[sectionHeadInstanceId];
            return GetInstanceIds(sectionId);
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
                _mapNodeByInstance,
                _depthByInstance,
                _sectionsByInfiniteNode,
                _floorSpecByMapNode.Where(p => IsInfiniteSection(p.Key)));

        private int RequireBlueprintGraphDepth(Id<MapNode> graphMapNodeId)
        {
            if (!_blueprint.GetDepthByGraphNode().TryGetValue(graphMapNodeId, out var depth))
            {
                throw new InvalidOperationException(
                    $"{_blueprint.Name}: MapNode {graphMapNodeId} is not reachable from start (no depth assigned).");
            }

            return depth;
        }

        private void EnsureGraphNodeRegistered(Id<MapNode> graphMapNodeId)
        {
            if (GetInstanceIds(graphMapNodeId).Count > 0) return;

            if (!_floorSpecByMapNode.ContainsKey(graphMapNodeId))
            {
                _floorSpecByMapNode[graphMapNodeId] = _blueprint.CreateFloorSpec(graphMapNodeId);
            }

            RegisterInstances(graphMapNodeId, RequireBlueprintGraphDepth(graphMapNodeId));
        }

        private Id<MapNode> CreateInfiniteSection(Id<MapNode> infiniteGraphNodeId, Id<MapNode> predecessorId)
        {
            var sectionId = Id<MapNode>.Generate();
            _floorSpecByMapNode[sectionId] = _blueprint.CreateInfiniteSectionSpec(infiniteGraphNodeId);

            GetSections(infiniteGraphNodeId).Add(sectionId);
            _predecessorBySection[sectionId] = predecessorId;

            var depthFirst = GetDepthLast(predecessorId) + 1;
            RegisterInstances(sectionId, depthFirst);

            return sectionId;
        }

        private void RegisterInstances(Id<MapNode> ownerId, int depthFirst)
        {
            var repeat = _blueprint.GetRepeat(ResolveGraphNodeId(ownerId));
            for (int i = 0; i < repeat; i++)
            {
                var id = Id<IMap>.Generate();
                _mapNodeByInstance[id] = ownerId;
                _depthByInstance[id] = depthFirst + i;
            }
        }

        private int GetDepthLast(Id<MapNode> ownerId)
        {
            var depthFirst = IsInfiniteSection(ownerId)
                ? _depthByInstance[GetInstanceIds(ownerId)[0]]
                : RequireBlueprintGraphDepth(ownerId);
            return depthFirst + _blueprint.GetRepeat(ResolveGraphNodeId(ownerId)) - 1;
        }

        private Id<IMap> ReserveNextSection(Id<MapNode> fromId, Id<MapNode> infiniteGraphNodeId)
        {
            if (!CanReachInfiniteTemplate(fromId, infiniteGraphNodeId))
            {
                throw new InvalidOperationException(
                    $"MapNode {fromId} cannot reach infinite template {infiniteGraphNodeId}.");
            }

            var sections = GetSections(infiniteGraphNodeId);

            if (IsInfiniteSection(fromId))
            {
                var index = sections.IndexOf(fromId);
                if (index + 1 < sections.Count)
                    return GetInstanceIds(sections[index + 1])[0];

                var nextSectionId = CreateInfiniteSection(infiniteGraphNodeId, fromId);
                return GetInstanceIds(nextSectionId)[0];
            }

            foreach (var sectionId in sections)
            {
                if (_predecessorBySection.TryGetValue(sectionId, out var predecessor)
                    && predecessor.Equals(fromId))
                {
                    return GetInstanceIds(sectionId)[0];
                }
            }

            var createdSectionId = CreateInfiniteSection(infiniteGraphNodeId, fromId);
            return GetInstanceIds(createdSectionId)[0];
        }

        private bool CanReachInfiniteTemplate(Id<MapNode> fromId, Id<MapNode> infiniteGraphNodeId)
        {
            if (IsInfiniteSection(fromId) && GetInfiniteGraphId(fromId).Equals(infiniteGraphNodeId))
                return true;

            var graphNodeId = ResolveGraphNodeId(fromId);
            return _blueprint.GetNextMapNodeIds(graphNodeId).Any(id => id == infiniteGraphNodeId);
        }

        private List<Id<MapNode>> GetSections(Id<MapNode> infiniteGraphNodeId)
        {
            if (!_sectionsByInfiniteNode.TryGetValue(infiniteGraphNodeId, out var sections))
            {
                sections = new List<Id<MapNode>>();
                _sectionsByInfiniteNode[infiniteGraphNodeId] = sections;
            }

            return sections;
        }

        private IReadOnlyList<Id<IMap>> GetInstanceIds(Id<MapNode> mapNodeId) =>
            _mapNodeByInstance
                .Where(p => p.Value.Equals(mapNodeId))
                .OrderBy(p => _depthByInstance[p.Key])
                .Select(p => p.Key)
                .ToList();

        private bool TryGetInfiniteGraphId(Id<MapNode> sectionId, out Id<MapNode> infiniteGraphId)
        {
            foreach (var (infGraphId, sectionIds) in _sectionsByInfiniteNode)
            {
                if (!sectionIds.Contains(sectionId)) continue;
                infiniteGraphId = infGraphId;
                return true;
            }

            infiniteGraphId = default!;
            return false;
        }

        private bool IsInfiniteSection(Id<MapNode> mapNodeId) => TryGetInfiniteGraphId(mapNodeId, out _);

        private Id<MapNode> GetInfiniteGraphId(Id<MapNode> sectionId) =>
            TryGetInfiniteGraphId(sectionId, out var infiniteGraphId)
                ? infiniteGraphId
                : throw new InvalidOperationException($"Section {sectionId} is not registered.");
    }
}
