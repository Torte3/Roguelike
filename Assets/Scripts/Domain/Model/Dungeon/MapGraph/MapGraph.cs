using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using XNode;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Domain.Model.Dungeon
{
    [CreateAssetMenu]
    public class MapGraph : NodeGraph
    {
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (nodes == null)
            {
                return;
            }

            var changed = false;

            ValidateStartMapNode();
            if (ValidateUniqueMapNodeIds())
                changed = true;

            if (changed)
            {
                EditorUtility.SetDirty(this);
            }
        }

        private void ValidateStartMapNode()
        {
            var startNodes = nodes.OfType<StartMapNode>().Where(n => n != null).ToList();
            if (startNodes.Count != 1)
            {
                Debug.LogError(
                    $"{name}: StartMapNode はグラフに1つ必要です（現在 {startNodes.Count} 個）。",
                    this);
                return;
            }

            var firstMaps = GetMapNodesConnectedFromStart(startNodes[0]).ToList();
            if (firstMaps.Count != 1)
            {
                Debug.LogError(
                    $"{name}: StartMapNode の出力を、最初の MapNode の _prevMap に1つだけ接続してください（現在 {firstMaps.Count} 個）。",
                    this);
            }
        }

        private bool ValidateUniqueMapNodeIds()
        {
            var changed = false;
            var groups = GetAllBlueprints()
                .GroupBy(b => b.NodeId)
                .Where(g => g.Count() > 1);
            foreach (var group in groups)
            {
                foreach (var blueprint in group.Skip(1))
                {
                    if (TryRegenerateMapNodeId(blueprint.Node))
                    {
                        changed = true;
                        Debug.LogWarning(
                            $"{name}: 重複した MapNode ID を再生成しました ({blueprint.Node.name})。",
                            blueprint.Node);
                    }
                }
            }

            return changed;
        }

        private static bool TryRegenerateMapNodeId(Node node)
        {
            switch (node)
            {
                case MapNode mapNode:
                    mapNode.RegenerateMapNodeId();
                    return true;
                case InfiniteMapNode infiniteMapNode:
                    infiniteMapNode.RegenerateMapNodeId();
                    return true;
                default:
                    return false;
            }
        }

        [ContextMenu("全 MapNode ID を再生成")]
        private void RegenerateAllMapNodeIdsContextMenu() => RegenerateAllMapNodeIds();

        public void RegenerateAllMapNodeIds()
        {
            if (nodes == null) return;

            var count = 0;
            foreach (var blueprint in GetAllBlueprints())
            {
                if (TryRegenerateMapNodeId(blueprint.Node))
                    count++;
            }

            EditorUtility.SetDirty(this);
            Debug.LogWarning(
                $"{name}: {count} 個の MapNode / InfiniteMapNode ID を再生成しました。",
                this);
        }
#endif

        public override Node CopyNode(Node original)
        {
            var node = base.CopyNode(original);
            switch (node)
            {
                case MapNode mapNode:
                    mapNode.RegenerateMapNodeId();
                    break;
                case InfiniteMapNode infiniteMapNode:
                    infiniteMapNode.RegenerateMapNodeId();
                    break;
            }

            return node;
        }

        public IEnumerable<IMapNodeBlueprint> GetAllBlueprints()
        {
            if (nodes == null) yield break;
            foreach (var node in nodes)
            {
                if (node is IMapNodeBlueprint blueprint)
                    yield return blueprint;
            }
        }

        public IMapNodeBlueprint GetBlueprint(Id<MapNode> id)
        {
            return GetAllBlueprints().First(b => b.NodeId == id);
        }

        public bool IsGraphNode(Id<MapNode> id) => GetAllBlueprints().Any(b => b.NodeId == id);

        public bool IsInfiniteTemplate(Id<MapNode> id)
        {
            return GetBlueprint(id).Node is InfiniteMapNode;
        }

        public MapNode GetMapNode(Id<MapNode> id) => (MapNode)GetBlueprint(id).Node;

        public InfiniteMapNode GetInfiniteMapNode(Id<MapNode> id) => (InfiniteMapNode)GetBlueprint(id).Node;

        public Id<MapNode> GetStartMapNodeId()
        {
            var startNode = nodes.OfType<StartMapNode>().First(n => n != null);
            return GetMapNodesConnectedFromStart(startNode).First().NodeId;
        }

        private IEnumerable<MapNode> GetMapNodesConnectedFromStart(StartMapNode startNode)
        {
            if (nodes == null) yield break;

            foreach (var mapNode in nodes.OfType<MapNode>())
            {
                if (mapNode == null) continue;
                if (mapNode.GetInputPort("_prevMap").GetConnections().Any(c => c.node == startNode))
                    yield return mapNode;
            }
        }

        public int GetRepeat(Id<MapNode> graphNodeId) => GetBlueprint(graphNodeId).Repeat;

        public IEnumerable<Id<MapNode>> GetNextMapNodeIds(Id<MapNode> id)
        {
            if (IsInfiniteTemplate(id)) yield break;

            var node = GetBlueprint(id).Node;
            foreach (var targetId in ResolveOutputPortTargets(node.GetOutputPort("_nextMap")))
                yield return targetId;
        }

        public IEnumerable<Id<MapNode>> GetPrevMapNodeIds(Id<MapNode> id)
        {
            var node = GetBlueprint(id).Node;
            foreach (var prevId in ResolveInputPortSources(node.GetInputPort("_prevMap")))
                yield return prevId;
        }

        public IEnumerable<Id<MapNode>> GetTeleportInMapNodeIds(Id<MapNode> id)
        {
            var node = GetBlueprint(id).Node;
            foreach (var sourceId in ResolveInputPortSources(node.GetInputPort("_teleportIn")))
                yield return sourceId;
        }

        public IEnumerable<Id<MapNode>> GetTeleportOutMapNodeIds(Id<MapNode> id)
        {
            if (IsInfiniteTemplate(id)) yield break;

            var node = GetBlueprint(id).Node;
            foreach (var targetId in ResolveOutputPortTargets(node.GetOutputPort("_teleportOut")))
                yield return targetId;
        }

        private static IEnumerable<Id<MapNode>> ResolveOutputPortTargets(NodePort outputPort)
        {
            foreach (var connection in outputPort.GetConnections())
            {
                if (connection.node is IMapNodeBlueprint blueprint)
                    yield return blueprint.NodeId;
            }
        }

        private static IEnumerable<Id<MapNode>> ResolveInputPortSources(NodePort inputPort)
        {
            foreach (var connection in inputPort.GetConnections())
            {
                if (connection.node is IMapNodeBlueprint blueprint)
                    yield return blueprint.NodeId;
            }
        }
    }
}
