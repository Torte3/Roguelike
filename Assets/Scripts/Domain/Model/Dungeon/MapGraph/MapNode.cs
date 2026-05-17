#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Utilities.Table;
using XNode;
using System;
using Sirenix.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Domain.Model.Dungeon
{
    [CreateNodeMenu("Map"), NodeWidth(260)]
    public class MapNode : Node
    {
        [MinValue(1)]
#if UNITY_EDITOR
        [OnValueChanged(nameof(OnRepeatChanged))]
#endif
        public int Repeat = 1;
        
        [Input(ShowBackingValue.Never, connectionType: ConnectionType.Override), SerializeField]
        [InfoBox("Connection to SectionData is required.", InfoMessageType.Error, VisibleIf = nameof(_isSectionUnconnected))]
        private SectionData? _sectionData = null;
        [Input(ShowBackingValue.Never, connectionType: ConnectionType.Override), SerializeField]
        [InfoBox("No enemies spawn on this map.", InfoMessageType.Info, VisibleIf = nameof(_isEnemiesUnconnected))]
        private Table<EnemyData>? _enemies = null;
        public List<EnemyData> Boss;
        private bool HasBoss => Boss.Count > 0;
        [ShowIf(nameof(HasBoss))]
        [SerializeField] private List<ItemDataSerializable> _bossReward;
        public List<IItemData> BossReward => _bossReward.Select(r => r.Value).ToList();
        public FloorData FloorData;

        [Input(ShowBackingValue.Never), SerializeField]
        [InfoBox("This map has no previous map.", InfoMessageType.Info, VisibleIf = nameof(_isPrevMapUnconnected))]
        private int _prevMap;
        [Output(ShowBackingValue.Never), SerializeField]
        [InfoBox("This map has no next map.", InfoMessageType.Info, VisibleIf = nameof(_isNextMapUnconnected))]
        private int _nextMap;
        [Input(ShowBackingValue.Never), SerializeField]
        private int _teleportIn;
        [Output, SerializeField]
        private int _teleportOut;
        [SerializeField]
        private bool _isGoal = false;

        private bool _isPrevMapUnconnected => !GetInputPort(nameof(_prevMap)).IsConnected;
        private bool _isNextMapUnconnected => !GetOutputPort(nameof(_nextMap)).IsConnected;
        private bool _isSectionUnconnected => !GetInputPort(nameof(_sectionData)).IsConnected;
        private bool _isEnemiesUnconnected => !GetInputPort(nameof(_enemies)).IsConnected;

        public int GetIndex(Id<IMap> mapId)
        {
            var index = Array.IndexOf(_mapIds, mapId.ToString());
            if (index == -1)
            {
                throw new ArgumentException("MapId not found");
            }
            return index;
        }
        public bool ContainsMapId(Id<IMap> mapId) => _mapIds.Contains(mapId.ToString());
        public IEnumerable<Id<IMap>> PrevMapIds(Id<IMap> mapId)
        {
            var index = GetIndex(mapId);
            if (index == 0)
            {
                return GetInputPort(nameof(_prevMap))
                    .GetConnections()
                    .Select(connection => connection.node)
                    .OfType<MapNode>()
                    .Select(node => node.LastMapId);
            }
            else
            {
                return new Id<IMap>[] { new(_mapIds[index - 1]) };
            }
        }

        public IEnumerable<Id<IMap>> TeleportInMapIds(Id<IMap> mapId)
        {
            var index = GetIndex(mapId);
            if (index == 0) // 最初のマップの場合のみ
            {
                return GetInputPort(nameof(_teleportIn))
                    .GetConnections()
                    .Select(connection => connection.node)
                    .OfType<MapNode>()
                    .Select(node => node.LastMapId);
            }
            return Enumerable.Empty<Id<IMap>>();
        }
        public IEnumerable<Id<IMap>> NextMapIds(Id<IMap> mapId)
        {
            var index = GetIndex(mapId);
            if (index == _mapIds.Length - 1)
            {
                return GetOutputPort(nameof(_nextMap))
                    .GetConnections()
                    .Select(connection => connection.node)
                    .OfType<MapNode>()
                    .Select(node => node.FirstMapId);
            }
            else
            {
                return new Id<IMap>[] { new(_mapIds[index + 1]) };
            }
        }

        public IEnumerable<Id<IMap>> TeleportOutMapIds(Id<IMap> mapId)
        {
            var index = GetIndex(mapId);
            if (index == _mapIds.Length - 1) // 最後のマップの場合のみ
            {
                return GetOutputPort(nameof(_teleportOut))
                    .GetConnections()
                    .Select(connection => connection.node)
                    .OfType<MapNode>()
                    .Select(node => node.FirstMapId);
            }
            return Enumerable.Empty<Id<IMap>>();
        }

        public SectionData SectionData => GetInputValue<SectionData>(nameof(_sectionData));
        public Table<EnemyData> Enemies => GetInputValue<Table<EnemyData>>(nameof(_enemies), new());

        public override object GetValue(NodePort port)
        {
            return Math.Max(GetInputValues(nameof(_prevMap), 0).Max() + 1, GetInputValues(nameof(_teleportIn), 0).Max()) + Repeat - 1;
        }

        [SerializeField, HideInInspector] private string[] _mapIds;
        public Id<IMap> FirstMapId => new(_mapIds.First());
        public Id<IMap> LastMapId => new(_mapIds.Last());
        public bool IsStartMapId(Id<IMap> mapId) =>
            PrevMapIds(mapId).Count() == 0
            && TeleportInMapIds(mapId).Count() == 0;
        public int Depth(Id<IMap> mapId) =>
            Math.Max(GetInputValues(nameof(_prevMap), 0).Max() + 1, GetInputValues(nameof(_teleportIn), 0).Max())
            + GetIndex(mapId);

#if UNITY_EDITOR
        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            base.OnCreateConnection(from, to);
            ClearOverrideBackingFieldsWhenConnected();
        }

        private void OnValidate()
        {
            ClearOverrideBackingFieldsWhenConnected();
        }

        internal bool ClearOverrideBackingFieldsWhenConnected()
        {
            Debug.Log("ClearOverrideBackingFieldsWhenConnected");
            var changed = false;
            if (GetInputPort(nameof(_sectionData)).IsConnected && _sectionData != null)
            {
                _sectionData = null;
                changed = true;
            }

            if (GetInputPort(nameof(_enemies)).IsConnected && _enemies != null)
            {
                _enemies = null;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(this);
            }

            return changed;
        }

        protected override void Init()
        {
            base.Init();
            _sectionData = null;
            _enemies = null;
            
            var mapGraph = graph as MapGraph;
            var existingIds = mapGraph.nodes.OfType<MapNode>()
                .Where(node => node != this)
                .SelectMany(node => node._mapIds)
                .ToHashSet();
            
            if (_mapIds.IsNullOrEmpty() || _mapIds.Any(id => existingIds.Contains(id)))
            {
                _mapIds = new string[Repeat];
                for (int i = 0; i < Repeat; i++)
                {
                    _mapIds[i] = Id<IMap>.Generate().ToString();
                }
                EditorUtility.SetDirty(this);
            }
        }
        private void OnRepeatChanged()
        {
            var mapIds = new string[Repeat];
            for (int i = 0; i < Repeat; i++)
            {
                mapIds[i] = string.IsNullOrEmpty(i < _mapIds.Length ? _mapIds[i] : null) ? Id<IMap>.Generate().ToString() : _mapIds[i];
            }
            _mapIds = mapIds;
            EditorUtility.SetDirty(this);
        }
        [ShowInInspector, TextArea(1, 1)]
        private string _infoText
        {
            get
            {
                if (_mapIds.IsNullOrEmpty()) return "Error";

                var isStartMap = IsStartMapId(new Id<IMap>(_mapIds[0]));
                var minDepth = Depth(FirstMapId);
                var maxDepth = minDepth + Repeat - 1;
                return $"{(isStartMap ? "Start" : "")} Depth: {minDepth}-{maxDepth}";
            }
        }
#endif
    }
}