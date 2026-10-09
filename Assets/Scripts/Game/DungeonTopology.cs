#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Dungeon;
using Domain.Model.Map;
using R3;
using Utilities;

namespace Game
{
    /// <summary>
    /// マップ間の接続（階段・魔法陣）、MapId 実体、無限セクションのグラフ状態を管理する。
    /// </summary>
    internal class DungeonTopology
    {
        private readonly IDungeonBlueprintData _blueprint;

        private readonly Dictionary<Id<IMap>, Id<MapNode>> _mapNodeByMapId;
        private readonly Dictionary<Id<IMap>, int> _depthByMapId;
        private readonly Dictionary<Id<MapNode>, List<Id<MapNode>>> _sectionsByInfiniteNode;
        private readonly Dictionary<Id<MapNode>, List<Id<MapNode>>> _bossSectionsByInfiniteNode;
        private readonly Dictionary<Id<MapNode>, Id<MapNode>> _predecessorBySection = new();
        private readonly Subject<InfiniteSectionCreatedMessage> _infiniteSectionCreated = new();
        private readonly Subject<BlueprintGraphNodeInitializedMessage> _blueprintGraphNodeInitialized = new();

        public Observable<InfiniteSectionCreatedMessage> OnInfiniteSectionCreated => _infiniteSectionCreated;

        public Observable<BlueprintGraphNodeInitializedMessage> OnBlueprintGraphNodeInitialized =>
            _blueprintGraphNodeInitialized;

        public DungeonTopology(IDungeonBlueprintData blueprint)
        {
            _blueprint = blueprint;
            _mapNodeByMapId = new Dictionary<Id<IMap>, Id<MapNode>>();
            _depthByMapId = new Dictionary<Id<IMap>, int>();
            _sectionsByInfiniteNode = new Dictionary<Id<MapNode>, List<Id<MapNode>>>();
            _bossSectionsByInfiniteNode = new Dictionary<Id<MapNode>, List<Id<MapNode>>>();
        }

        public DungeonTopology(
            IDungeonBlueprintData blueprint,
            Dictionary<Id<IMap>, Id<MapNode>> mapNodeByMapId,
            Dictionary<Id<IMap>, int> depthByMapId,
            Dictionary<Id<MapNode>, List<Id<MapNode>>> sectionsByInfiniteNode,
            Dictionary<Id<MapNode>, List<Id<MapNode>>> bossSectionsByInfiniteNode)
        {
            _blueprint = blueprint;
            _mapNodeByMapId = mapNodeByMapId;
            _depthByMapId = depthByMapId;
            _sectionsByInfiniteNode = sectionsByInfiniteNode;
            _bossSectionsByInfiniteNode = bossSectionsByInfiniteNode;
        }

        public IReadOnlyDictionary<Id<IMap>, Id<MapNode>> MapNodeByMapId => _mapNodeByMapId;

        public IReadOnlyDictionary<Id<IMap>, int> DepthByMapId => _depthByMapId;

        public IReadOnlyDictionary<Id<MapNode>, List<Id<MapNode>>> SectionsByInfiniteNode => _sectionsByInfiniteNode;

        public IReadOnlyDictionary<Id<MapNode>, List<Id<MapNode>>> BossSectionsByInfiniteNode =>
            _bossSectionsByInfiniteNode;

        public IEnumerable<Id<MapNode>> AllMapNodeIds => _mapNodeByMapId.Values.Distinct();

        public int GetDepth(Id<IMap> mapId) => _depthByMapId[mapId];

        public Id<MapNode> GetMapNodeId(Id<IMap> mapId) => _mapNodeByMapId[mapId];

        public bool TryGetMapNodeId(Id<IMap> mapId, out Id<MapNode> mapNodeId) =>
            _mapNodeByMapId.TryGetValue(mapId, out mapNodeId);

        public List<MapConnection> GetDestinations(Id<IMap> mapId)
        {
            var ownerId = GetMapNodeId(mapId);
            var connections = new List<MapConnection>();

            if (IsInfiniteBossSection(ownerId))
            {
                AddBossFloorConnections(connections, ownerId);
                return connections;
            }

            var ids = GetMapIds(ownerId);
            var index = ids.IndexOf(mapId);

            var isFirst = index == 0;
            var isLast = index == ids.Count - 1;

            if (!isLast)
                connections.Add(new MapConnection(MovementEntityType.DownStairs, ids[index + 1]));

            if (!isFirst)
                connections.Add(new MapConnection(MovementEntityType.UpStairs, ids[index - 1]));

            if (isFirst)
                AddFirstFloorConnections(connections, ownerId);

            if (isLast)
                AddLastFloorConnections(connections, ownerId);

            return connections;
        }

        public bool IsInfiniteSection(Id<MapNode> mapNodeId) =>
            _sectionsByInfiniteNode.Values.Any(sectionIds => sectionIds.Contains(mapNodeId));

        public bool IsInfiniteBossSection(Id<MapNode> mapNodeId) =>
            _bossSectionsByInfiniteNode.Values.Any(bossSectionIds => bossSectionIds.Contains(mapNodeId));

        public Id<MapNode> GetBossSectionId(Id<MapNode> normalSectionId)
        {
            var infiniteGraphId = ResolveGraphNodeId(normalSectionId);
            var index = _sectionsByInfiniteNode[infiniteGraphId].IndexOf(normalSectionId);
            return _bossSectionsByInfiniteNode[infiniteGraphId][index];
        }

        public void RestorePredecessorsBySection()
        {
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

                    var mapIds = GetMapIds(sectionId);
                    if (mapIds.Count == 0) continue;

                    var expectedPredLast = GetDepth(mapIds[0]) - 1;
                    var predecessor = depthByGraph.Keys
                        .Where(graphId => CanReachInfiniteTemplate(graphId, infiniteGraphNodeId))
                        .Where(graphId => GetDepthLast(graphId) == expectedPredLast)
                        .OrderByDescending(graphId => GetMapIds(graphId).Count > 0)
                        .ThenBy(graphId => depthByGraph[graphId])
                        .FirstOrDefault();
                    if (predecessor != null)
                        _predecessorBySection[sectionId] = predecessor;
                }
            }
        }

        public bool TryGetInfiniteGraphId(Id<MapNode> mapNodeId, out Id<MapNode> infiniteGraphId)
        {
            foreach (var (infGraphId, sectionIds) in _sectionsByInfiniteNode)
            {
                if (sectionIds.Contains(mapNodeId))
                {
                    infiniteGraphId = infGraphId;
                    return true;
                }
            }

            foreach (var (infGraphId, bossSectionIds) in _bossSectionsByInfiniteNode)
            {
                if (bossSectionIds.Contains(mapNodeId))
                {
                    infiniteGraphId = infGraphId;
                    return true;
                }
            }

            infiniteGraphId = default!;
            return false;
        }

        public IReadOnlyList<Id<IMap>> GetMapIds(Id<MapNode> mapNodeId)
        {
            if (!HasAnyMapForMapNode(mapNodeId) && _blueprint.IsGraphMapNode(mapNodeId))
            {
                _blueprintGraphNodeInitialized.OnNext(new BlueprintGraphNodeInitializedMessage(mapNodeId));
                RegisterMapIds(mapNodeId, RequireBlueprintGraphDepth(mapNodeId));
            }

            return _mapNodeByMapId
                .Where(p => p.Value.Equals(mapNodeId))
                .OrderBy(p => _depthByMapId[p.Key])
                .Select(p => p.Key)
                .ToList();
        }

        private void AddBossFloorConnections(List<MapConnection> connections, Id<MapNode> ownerId)
        {
            var normalSectionId = GetNormalSectionId(ownerId);
            connections.Add(new MapConnection(
                MovementEntityType.MagicCircle,
                GetMapIds(normalSectionId)[^1]));

            var graphNodeId = ResolveGraphNodeId(ownerId);
            foreach (var teleportOut in _blueprint.GetTeleportOutMapNodeIds(graphNodeId))
            {
                connections.Add(new MapConnection(
                    MovementEntityType.MagicCircle,
                    GetMapIds(teleportOut)[0]));
            }
        }

        private void AddFirstFloorConnections(List<MapConnection> connections, Id<MapNode> ownerId)
        {
            var graphNodeId = ResolveGraphNodeId(ownerId);
            foreach (var prevGraphId in GetPrevMapNodeIds(ownerId, graphNodeId))
            {
                var prevIds = GetMapIds(prevGraphId);
                connections.Add(new MapConnection(MovementEntityType.UpStairs, prevIds[^1]));
            }

            foreach (var teleportIn in _blueprint.GetTeleportInMapNodeIds(graphNodeId))
            {
                var targetIds = GetMapIds(teleportIn);
                connections.Add(new MapConnection(MovementEntityType.MagicCircle, targetIds[^1]));
            }
        }

        private void AddLastFloorConnections(List<MapConnection> connections, Id<MapNode> ownerId)
        {
            var graphNodeId = ResolveGraphNodeId(ownerId);
            switch (GetMapNodeKind(ownerId))
            {
                case DungeonMapNodeKind.InfiniteNormal:
                    connections.Add(new MapConnection(
                        MovementEntityType.DownStairs,
                        GetMapIds(ReserveNextSection(ownerId, graphNodeId))[0]));
                    connections.Add(new MapConnection(
                        MovementEntityType.MagicCircle,
                        GetMapIds(GetBossSectionId(ownerId))[0]));
                    break;
                case DungeonMapNodeKind.FiniteBlueprint:
                    foreach (var nextGraphId in _blueprint.GetNextMapNodeIds(ownerId))
                    {
                        var dest = _blueprint.IsInfiniteTemplate(nextGraphId)
                            ? GetMapIds(ReserveNextSection(ownerId, nextGraphId))[0]
                            : GetMapIds(nextGraphId)[0];
                        connections.Add(new MapConnection(MovementEntityType.DownStairs, dest));
                    }
                    break;
            }

            foreach (var teleportOut in _blueprint.GetTeleportOutMapNodeIds(graphNodeId))
            {
                connections.Add(new MapConnection(
                    MovementEntityType.MagicCircle,
                    GetMapIds(teleportOut)[0]));
            }
        }

        private IEnumerable<Id<MapNode>> GetPrevMapNodeIds(Id<MapNode> ownerId, Id<MapNode> graphNodeId)
        {
            if (IsInfiniteSection(ownerId)
                && _predecessorBySection.TryGetValue(ownerId, out var predecessorId))
            {
                yield return predecessorId;
                yield break;
            }

            foreach (var prevId in _blueprint.GetPrevMapNodeIds(graphNodeId))
                yield return prevId;
        }

        private Id<MapNode> ReserveNextSection(Id<MapNode> fromId, Id<MapNode> infiniteGraphNodeId)
        {
            if (!CanReachInfiniteTemplate(fromId, infiniteGraphNodeId))
            {
                throw new InvalidOperationException(
                    $"MapNode {fromId} cannot reach infinite template {infiniteGraphNodeId}.");
            }

            if (IsInfiniteSection(fromId))
            {
                var sectionIds = _sectionsByInfiniteNode[infiniteGraphNodeId];
                var index = sectionIds.IndexOf(fromId);
                if (index + 1 < sectionIds.Count)
                    return sectionIds[index + 1];

                return CreateInfiniteSection(infiniteGraphNodeId, fromId);
            }

            if (_sectionsByInfiniteNode.TryGetValue(infiniteGraphNodeId, out var registeredSections))
            {
                foreach (var sectionId in registeredSections)
                {
                    if (_predecessorBySection.TryGetValue(sectionId, out var predecessor)
                        && predecessor.Equals(fromId))
                    {
                        return sectionId;
                    }
                }
            }

            return CreateInfiniteSection(infiniteGraphNodeId, fromId);
        }

        private Id<MapNode> CreateInfiniteSection(Id<MapNode> infiniteGraphNodeId, Id<MapNode> predecessorId)
        {
            var normalSectionId = Id<MapNode>.Generate();
            var bossSectionId = Id<MapNode>.Generate();

            if (!_sectionsByInfiniteNode.ContainsKey(infiniteGraphNodeId))
            {
                _sectionsByInfiniteNode[infiniteGraphNodeId] = new List<Id<MapNode>>();
                _bossSectionsByInfiniteNode[infiniteGraphNodeId] = new List<Id<MapNode>>();
            }

            _sectionsByInfiniteNode[infiniteGraphNodeId].Add(normalSectionId);
            _bossSectionsByInfiniteNode[infiniteGraphNodeId].Add(bossSectionId);
            _predecessorBySection[normalSectionId] = predecessorId;

            var (normal, boss) = _blueprint.PickRandomInfiniteSectionFloorSpecs(infiniteGraphNodeId);
            var depthFirst = GetDepthLast(predecessorId) + 1;
            RegisterMapIds(normalSectionId, depthFirst);
            RegisterBossMapId(bossSectionId, infiniteGraphNodeId, depthFirst);
            _infiniteSectionCreated.OnNext(
                new InfiniteSectionCreatedMessage(normalSectionId, bossSectionId, normal, boss));

            return normalSectionId;
        }

        private void RegisterBossMapId(Id<MapNode> bossSectionId, Id<MapNode> infiniteGraphNodeId, int depthFirst)
        {
            var bossMapId = Id<IMap>.Generate();
            _mapNodeByMapId[bossMapId] = bossSectionId;
            _depthByMapId[bossMapId] = depthFirst + _blueprint.GetRepeat(infiniteGraphNodeId);
        }

        private Id<MapNode> GetNormalSectionId(Id<MapNode> bossSectionId)
        {
            var infiniteGraphId = ResolveGraphNodeId(bossSectionId);
            var index = _bossSectionsByInfiniteNode[infiniteGraphId].IndexOf(bossSectionId);
            return _sectionsByInfiniteNode[infiniteGraphId][index];
        }

        private void RegisterMapIds(Id<MapNode> ownerId, int depthFirst)
        {
            var repeat = _blueprint.GetRepeat(ResolveGraphNodeId(ownerId));
            for (int i = 0; i < repeat; i++)
            {
                var id = Id<IMap>.Generate();
                _mapNodeByMapId[id] = ownerId;
                _depthByMapId[id] = depthFirst + i;
            }
        }

        private bool HasAnyMapForMapNode(Id<MapNode> mapNodeId) =>
            _mapNodeByMapId.Any(p => p.Value.Equals(mapNodeId));

        private int GetDepthLast(Id<MapNode> ownerId)
        {
            if (IsInfiniteBossSection(ownerId))
                return _depthByMapId[GetMapIds(ownerId)[0]];

            var depthFirst = IsInfiniteSection(ownerId)
                ? _depthByMapId[GetMapIds(ownerId)[0]]
                : RequireBlueprintGraphDepth(ownerId);
            return depthFirst + _blueprint.GetRepeat(ResolveGraphNodeId(ownerId)) - 1;
        }

        private int RequireBlueprintGraphDepth(Id<MapNode> graphMapNodeId)
        {
            if (!_blueprint.GetDepthByGraphNode().TryGetValue(graphMapNodeId, out var depth))
            {
                throw new InvalidOperationException(
                    $"{_blueprint.Name}: MapNode {graphMapNodeId} is not reachable from start (no depth assigned).");
            }

            return depth;
        }

        private DungeonMapNodeKind GetMapNodeKind(Id<MapNode> mapNodeId)
        {
            if (IsInfiniteSection(mapNodeId)) return DungeonMapNodeKind.InfiniteNormal;
            if (IsInfiniteBossSection(mapNodeId)) return DungeonMapNodeKind.InfiniteBoss;
            return DungeonMapNodeKind.FiniteBlueprint;
        }

        private Id<MapNode> ResolveGraphNodeId(Id<MapNode> ownerId) =>
            TryGetInfiniteGraphId(ownerId, out var infiniteGraphId) ? infiniteGraphId : ownerId;

        private bool CanReachInfiniteTemplate(Id<MapNode> fromId, Id<MapNode> infiniteGraphNodeId)
        {
            if (TryGetInfiniteGraphId(fromId, out var graphId) && graphId.Equals(infiniteGraphNodeId))
                return true;

            var graphNodeId = ResolveGraphNodeId(fromId);
            return _blueprint.GetNextMapNodeIds(graphNodeId).Any(id => id == infiniteGraphNodeId);
        }
    }
}
