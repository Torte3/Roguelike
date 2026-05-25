#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using RandomDungeonWithBluePrint;
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
    [CreateNodeMenu("Infinite Map"), NodeWidth(280)]
    public class InfiniteMapNode : Node, IMapNodeBlueprint
    {
        public const int DefaultRepeat = 3;

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

        [MinValue(1)] public int Repeat = DefaultRepeat;

        int IMapNodeBlueprint.Repeat => Repeat;

        Node IMapNodeBlueprint.Node => this;

        [Required] public FloorData FloorData = null!;
        [Required] public FloorData BossFloorData = null!;

        [SerializeField]
        [RequiredListLength(1, null)]
        private List<InfiniteSectionDefinition> _sections = new();

        [Required] public EnemyTableData EnemyTable = null!;

        public Table<EnemyData> Enemies => EnemyTable != null ? EnemyTable.Enemies : new();

        [MinValue(1)] public int EnemyPickCount = 3;

        internal IReadOnlyList<InfiniteSectionDefinition> SectionDefinitions => _sections;

        [Input(ShowBackingValue.Never, typeConstraint: TypeConstraint.Strict), SerializeField]
        private StairsLink _prevMap;
        [Input(ShowBackingValue.Never, typeConstraint: TypeConstraint.Strict), SerializeField]
        private TeleportLink _teleportIn;

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
