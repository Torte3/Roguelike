using System.Linq;
using Domain.Model.Map;
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
            foreach (var mapNode in nodes.OfType<MapNode>())
            {
                if (mapNode != null && mapNode.ClearOverrideBackingFieldsWhenConnected())
                {
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(this);
            }
        }
#endif

        public Id<IMap> GetStartMapId()
        {
            return nodes.OfType<MapNode>().Where(node => node.IsStartMapId(node.FirstMapId)).First().FirstMapId;
        }
        public int GetMaxDepth()
        {
            return nodes.OfType<MapNode>().Max(node => node.Depth(node.LastMapId));
        }
        public MapNode GetMapNode(Id<IMap> mapId)
        {
            return nodes.OfType<MapNode>().First(node => node.ContainsMapId(mapId));
        }
    }
}