#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Item;
using UnityEngine;
using Utilities;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    public class StatisticsMemento
    {
        [field: SerializeField] public long PlayTime { get; private set; }
        [field: SerializeField] public int Turn { get; private set; }
        [field: SerializeField] public int MaxMapLevel { get; private set; }
        [field: SerializeField] public bool IsCheating { get; private set; }
        [field: SerializeField] public int TotalDamageReceived { get; private set; }
        [field: SerializeField] public int MaxDamageReceived { get; private set; }
        [field: SerializeField] public int TotalDamageDealt { get; private set; }
        [field: SerializeField] public int MaxDamageDealt { get; private set; }
        [field: SerializeField] public int TotalHealReceived { get; private set; }
        [field: SerializeField] public int MaxHealReceived { get; private set; }
        [field: SerializeField] public int StealCount { get; private set; }
        [field: SerializeField] public int MonsterHouseEnterCount { get; private set; }
        [field: SerializeField] public int CursedItemDiscoverCount { get; private set; }
        [SerializeField] private List<string> _discoveredCursedItemIds = new();
        public HashSet<Id<IItem>> DiscoveredCursedItemIds =>
            _discoveredCursedItemIds.Select(id => new Id<IItem>(id)).ToHashSet();
        [SerializeField] private SerializableDictionary<string, int> _itemUsedCountByBaseName;
        public Dictionary<string, int> ItemUsedCountByBaseName => _itemUsedCountByBaseName.ToDictionary();
        [SerializeField] private SerializableDictionary<string, int> _deathCountByCause;
        public Dictionary<string, int> DeathCountByCause => _deathCountByCause.ToDictionary();
        [SerializeField] private SerializableDictionary<string, int> _enemyTypeKilledCount;
        public Dictionary<string, int> EnemyTypeKilledCount => _enemyTypeKilledCount.ToDictionary();
        public StatisticsMemento(
            long playTime,
            int turn,
            int maxMapLevel,
            bool isCheating,
            int totalDamageReceived,
            int maxDamageReceived,
            int totalDamageDealt,
            int maxDamageDealt,
            int totalHealReceived,
            int maxHealReceived,
            int stealCount,
            int monsterHouseEnterCount,
            IEnumerable<Id<IItem>> discoveredCursedItemIds,
            Dictionary<string, int> itemUsedCountByBaseName,
            Dictionary<string, int> deathCountByCause,
            Dictionary<string, int> enemyTypeKilledCount)
        {
            PlayTime = playTime;
            Turn = turn;
            MaxMapLevel = maxMapLevel;
            IsCheating = isCheating;
            TotalDamageReceived = totalDamageReceived;
            MaxDamageReceived = maxDamageReceived;
            TotalDamageDealt = totalDamageDealt;
            MaxDamageDealt = maxDamageDealt;
            TotalHealReceived = totalHealReceived;
            MaxHealReceived = maxHealReceived;
            StealCount = stealCount;
            MonsterHouseEnterCount = monsterHouseEnterCount;
            _discoveredCursedItemIds = discoveredCursedItemIds.Select(id => id.ToString()).ToList();
            CursedItemDiscoverCount = _discoveredCursedItemIds.Count;
            _itemUsedCountByBaseName = itemUsedCountByBaseName.ToSerializable();
            _deathCountByCause = deathCountByCause.ToSerializable();
            _enemyTypeKilledCount = enemyTypeKilledCount.ToSerializable();
        }
    }
}