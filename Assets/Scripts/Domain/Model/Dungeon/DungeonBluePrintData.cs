#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using RandomDungeonWithBluePrint;
using Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Table;
using Random = UnityEngine.Random;

namespace Domain.Model.Dungeon
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Dungeon")]
    public class DungeonBluePrintData : ScriptableObject, IDungeonBlueprintData
    {
        public string Name => name;
        [SerializeField] private MapGraph _mapGraph;
        public Table<FieldBluePrint> Fields;
        public MasterItemDataBase MasterItemDataBase;
        public ItemCategoryWeight SpawnItem;
        public Table<TrapData> Traps;
        public Table<StatueData> Statues;
        [Required] public RarityWeightTable<WeaponPrefix> WeaponPrefixes;
        public Table<EnemyData> Npcs;

        [NonSerialized] private Dictionary<Id<MapNode>, int>? _depthByGraphNode;

#if UNITY_EDITOR
        private void OnValidate() => _depthByGraphNode = null;
#endif

        public Id<MapNode> GetStartMapNodeId() => _mapGraph.GetStartMapNodeId();

        public IEnumerable<Id<MapNode>> GetNextMapNodeIds(Id<MapNode> graphMapNodeId) =>
            _mapGraph.GetNextMapNodeIds(graphMapNodeId);

        public IEnumerable<Id<MapNode>> GetPrevMapNodeIds(Id<MapNode> graphMapNodeId) =>
            _mapGraph.GetPrevMapNodeIds(graphMapNodeId);

        public IEnumerable<Id<MapNode>> GetTeleportInMapNodeIds(Id<MapNode> graphMapNodeId) =>
            _mapGraph.GetTeleportInMapNodeIds(graphMapNodeId);

        public IEnumerable<Id<MapNode>> GetTeleportOutMapNodeIds(Id<MapNode> graphMapNodeId) =>
            _mapGraph.GetTeleportOutMapNodeIds(graphMapNodeId);

        public bool IsInfiniteTemplate(Id<MapNode> graphMapNodeId) =>
            _mapGraph.IsInfiniteTemplate(graphMapNodeId);

        public bool IsGraphMapNode(Id<MapNode> mapNodeId) => _mapGraph.IsGraphNode(mapNodeId);

        public int GetRepeat(Id<MapNode> graphMapNodeId) => _mapGraph.GetRepeat(graphMapNodeId);

        public int GetMaxFiniteDepth()
        {
            var start = GetStartMapNodeId();
            var memo = new Dictionary<Id<MapNode>, int>();
            return MaxFiniteDepthFrom(start, memo, new HashSet<Id<MapNode>>());
        }

        public IReadOnlyDictionary<Id<MapNode>, int> GetDepthByGraphNode()
        {
            _depthByGraphNode ??= ComputeDepthByGraphNode();
            return _depthByGraphNode;
        }

        private Dictionary<Id<MapNode>, int> ComputeDepthByGraphNode()
        {
            var dist = new Dictionary<Id<MapNode>, int>();
            var start = GetStartMapNodeId();
            dist[start] = 1;

            var finiteNodes = GetAllGraphMapNodeIds().Where(id => !IsInfiniteTemplate(id)).ToList();
            var changed = true;
            while (changed)
            {
                changed = false;
                foreach (var current in finiteNodes.Where(dist.ContainsKey).ToList())
                {
                    var depth = dist[current];
                    var repeat = GetRepeat(current);
                    var nextDepth = depth + repeat;
                    var teleportDepth = depth + repeat - 1;

                    foreach (var next in GetNextMapNodeIds(current))
                    {
                        if (IsInfiniteTemplate(next)) continue;
                        changed |= TryRelaxDepth(dist, next, nextDepth);
                    }

                    foreach (var next in GetTeleportOutMapNodeIds(current))
                    {
                        if (IsInfiniteTemplate(next)) continue;
                        changed |= TryRelaxDepth(dist, next, teleportDepth);
                    }

                    foreach (var target in finiteNodes)
                    {
                        if (!GetTeleportInMapNodeIds(target).Contains(current)) continue;
                        changed |= TryRelaxDepth(dist, target, teleportDepth);
                    }
                }
            }

            return dist;
        }

        private static bool TryRelaxDepth(Dictionary<Id<MapNode>, int> dist, Id<MapNode> node, int candidate)
        {
            if (dist.TryGetValue(node, out var existing) && existing <= candidate) return false;
            dist[node] = candidate;
            return true;
        }

        public IEnumerable<Id<MapNode>> GetAllGraphMapNodeIds()
        {
            if (_mapGraph == null)
            {
                throw new System.InvalidOperationException(
                    $"{name}: MapGraph is not assigned on DungeonBluePrintData.");
            }

            return _mapGraph.GetAllBlueprints().Where(b => b != null).Select(b => b.NodeId);
        }

        public FloorSpec CreateFloorSpec(Id<MapNode> graphMapNodeId)
        {
            var mapNode = _mapGraph.GetMapNode(graphMapNodeId);
            var field = mapNode.FloorData.Field ?? Fields.GetRandomItem();
            return FloorSpec.Build(
                this,
                mapNode.SectionData,
                mapNode.FloorData,
                field,
                mapNode.Enemies,
                mapNode.Boss,
                mapNode.BossReward);
        }

        public FloorSpec CreateInfiniteSectionSpec(Id<MapNode> infiniteGraphNodeId)
        {
            var infiniteNode = _mapGraph.GetInfiniteMapNode(infiniteGraphNodeId);
            var candidates = infiniteNode.SectionCandidates;
            var sectionData = candidates.Count > 0
                ? candidates[Random.Range(0, candidates.Count)]
                : throw new System.InvalidOperationException("InfiniteMapNode has no Section candidates");

            var field = infiniteNode.FloorData.Field ?? Fields.GetRandomItem();
            var pickedEnemies = PickEnemiesFromPool(infiniteNode.Enemies, infiniteNode.EnemyPickCount);
            var enemies = new Table<EnemyData>(pickedEnemies);
            return FloorSpec.Build(
                this,
                sectionData,
                infiniteNode.FloorData,
                field,
                enemies,
                new List<EnemyData>(),
                new List<IItemData>());
        }

        public FloorData GetInfiniteFloorData(Id<MapNode> infiniteGraphNodeId) =>
            _mapGraph.GetInfiniteMapNode(infiniteGraphNodeId).FloorData;

        public FloorSpec ApplyInfiniteSectionBossFloor(FloorSpec sectionSpec, Id<MapNode> infiniteGraphNodeId)
        {
            var infiniteNode = _mapGraph.GetInfiniteMapNode(infiniteGraphNodeId);
            if (infiniteNode.Boss is not { Count: > 0 })
                return sectionSpec;

            return sectionSpec with
            {
                Boss = infiniteNode.Boss,
                BossReward = infiniteNode.BossReward
            };
        }

        private int MaxFiniteDepthFrom(
            Id<MapNode> id,
            Dictionary<Id<MapNode>, int> memo,
            HashSet<Id<MapNode>> visiting)
        {
            if (memo.TryGetValue(id, out var cached))
                return cached;

            if (IsInfiniteTemplate(id))
            {
                memo[id] = 0;
                return 0;
            }

            if (visiting.Contains(id))
                return int.MaxValue;

            visiting.Add(id);

            var beyondCandidates = new List<int>();
            foreach (Id<MapNode> next in GetNextMapNodeIds(id))
            {
                beyondCandidates.Add(MaxFiniteDepthFrom(next, memo, visiting) + 1);
            }

            foreach (Id<MapNode> next in GetTeleportOutMapNodeIds(id))
            {
                beyondCandidates.Add(MaxFiniteDepthFrom(next, memo, visiting));
            }

            visiting.Remove(id);

            var beyond = beyondCandidates.Count > 0 ? beyondCandidates.Min() : 0;
            var total = GetRepeat(id) + beyond;
            memo[id] = total;
            return total;
        }

        private static List<EnemyData> PickEnemiesFromPool(Table<EnemyData> pool, int pickCount)
        {
            var count = Mathf.Min(pickCount, pool.Count);
            var used = new HashSet<EnemyData>();
            var picked = new List<EnemyData>();
            for (int i = 0; i < count; i++)
            {
                EnemyData enemy;
                var attempts = 0;
                do
                {
                    enemy = pool.GetRandomItem();
                    attempts++;
                } while (used.Contains(enemy) && attempts < 50 && used.Count < pool.Count);

                used.Add(enemy);
                picked.Add(enemy);
            }

            return picked;
        }

    }
}
