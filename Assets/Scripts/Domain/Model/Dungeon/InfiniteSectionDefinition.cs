#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Item;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Dungeon
{
    [System.Serializable]
    internal class InfiniteSectionDefinition
    {
        [Required] public SectionData Section = null!;

        public List<EnemyData> Boss = new();

        private bool HasBoss => Boss is { Count: > 0 };

        [ShowIf(nameof(HasBoss))]
        [SerializeField]
        private List<ItemDataSerializable> _bossReward = new();

        public List<IItemData> BossReward =>
            _bossReward == null ? new List<IItemData>() : _bossReward.Select(r => r.Value).ToList();
    }
}
