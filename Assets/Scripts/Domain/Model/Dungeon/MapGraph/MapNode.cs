#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Item;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Utilities.Table;
using XNode;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Domain.Model.Dungeon
{
    [CreateNodeMenu("Map"), NodeWidth(260)]
    public class MapNode : Node, IMapNodeBlueprint
    {
        [SerializeField, HideInInspector] private string _mapNodeId = "";

        public Id<MapNode> NodeId
        {
            get
            {
                EnsureMapNodeIdRuntime();
                return new Id<MapNode>(_mapNodeId);
            }
        }

        private void EnsureMapNodeIdRuntime()
        {
            if (!string.IsNullOrEmpty(_mapNodeId)) return;
            RegenerateMapNodeId();
        }

        internal void RegenerateMapNodeId()
        {
            _mapNodeId = Id<MapNode>.Generate().ToString();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        [MinValue(1)] public int Repeat = 1;

        int IMapNodeBlueprint.Repeat => Repeat;

        Node IMapNodeBlueprint.Node => this;

        [Required] public SectionData SectionData;
        [Required] public FloorData FloorData;
        public EnemyTableData EnemyTable;
        public Table<EnemyData> Enemies => EnemyTable?.Enemies ?? new();

        public List<EnemyData> Boss = new();
        private bool HasBoss => Boss.Count > 0;
        [ShowIf(nameof(HasBoss))]
        [SerializeField] private List<ItemDataSerializable> _bossReward = new();
        public List<IItemData> BossReward =>
            _bossReward == null ? new List<IItemData>() : _bossReward.Select(r => r.Value).ToList();

        [Input(ShowBackingValue.Never, typeConstraint: TypeConstraint.Strict), SerializeField]
        private StairsLink _prevMap;
        [Output(ShowBackingValue.Never, typeConstraint: TypeConstraint.Strict), SerializeField]
        private StairsLink _nextMap;
        [Input(ShowBackingValue.Never, typeConstraint: TypeConstraint.Strict), SerializeField]
        private TeleportLink _teleportIn;
        [Output(ShowBackingValue.Never, typeConstraint: TypeConstraint.Strict), SerializeField]
        private TeleportLink _teleportOut;

#if UNITY_EDITOR
        private void OnValidate() => EnsureMapNodeIdEditor();

        private void EnsureMapNodeIdEditor()
        {
            if (!string.IsNullOrEmpty(_mapNodeId)) return;
            RegenerateMapNodeId();
        }
#endif

        protected override void Init()
        {
            base.Init();
#if UNITY_EDITOR
            EnsureMapNodeIdEditor();
#endif
        }
    }
}
