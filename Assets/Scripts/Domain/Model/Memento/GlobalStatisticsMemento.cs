#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    public class GlobalStatisticsMemento
    {
        [field: SerializeField] public int MaxMapLevel { get; private set; }
        [field: SerializeField] public List<string> KnownItemNames { get; private set; }
        [SerializeField] private SerializableDictionary<string, int> _enemyTypeKilledCount;
        public Dictionary<string, int> EnemyTypeKilledCount => _enemyTypeKilledCount.ToDictionary();
        [field: SerializeField] public long TotalPlayTime { get; private set; }
        [field: SerializeField] public int TotalTurns { get; private set; }
        [field: SerializeField] public int TotalDamageReceived { get; private set; }
        [field: SerializeField] public int MaxDamageReceived { get; private set; }
        [field: SerializeField] public int TotalDamageDealt { get; private set; }
        [field: SerializeField] public int MaxDamageDealt { get; private set; }
        [field: SerializeField] public int TotalHealReceived { get; private set; }
        [field: SerializeField] public int MaxHealReceived { get; private set; }
        [field: SerializeField] public int TotalStealCount { get; private set; }
        [field: SerializeField] public int TotalMonsterHouseEnterCount { get; private set; }
        [field: SerializeField] public int TotalCursedItemDiscoverCount { get; private set; }
        [field: SerializeField] public bool HasShownFirstGameTutorial { get; private set; }
        [field: SerializeField] public bool HasShownShopTutorial { get; private set; }
        [field: SerializeField] public bool HasShownMagicCircleTutorial { get; private set; }
        [field: SerializeField] public bool HasShownFloor30Tutorial { get; private set; }
        [SerializeField] private SerializableDictionary<string, int> _itemUsedCountByBaseName;
        public Dictionary<string, int> ItemUsedCountByBaseName => _itemUsedCountByBaseName.ToDictionary();
        [SerializeField] private SerializableDictionary<string, int> _deathCountByCause;
        public Dictionary<string, int> DeathCountByCause => _deathCountByCause.ToDictionary();
        public GlobalStatisticsMemento(
            int maxMapLevel,
            List<string> knownItemNames,
            Dictionary<string, int> enemyTypeKilledCount,
            long totalPlayTime,
            int totalTurns,
            int totalDamageReceived,
            int maxDamageReceived,
            int totalDamageDealt,
            int maxDamageDealt,
            int totalHealReceived,
            int maxHealReceived,
            int totalStealCount,
            int totalMonsterHouseEnterCount,
            int totalCursedItemDiscoverCount,
            Dictionary<string, int> itemUsedCountByBaseName,
            Dictionary<string, int> deathCountByCause,
            bool hasShownFirstGameTutorial,
            bool hasShownShopTutorial,
            bool hasShownMagicCircleTutorial,
            bool hasShownFloor30Tutorial)
        {
            MaxMapLevel = maxMapLevel;
            KnownItemNames = knownItemNames;
            _enemyTypeKilledCount = enemyTypeKilledCount.ToSerializable();
            TotalPlayTime = totalPlayTime;
            TotalTurns = totalTurns;
            TotalDamageReceived = totalDamageReceived;
            MaxDamageReceived = maxDamageReceived;
            TotalDamageDealt = totalDamageDealt;
            MaxDamageDealt = maxDamageDealt;
            TotalHealReceived = totalHealReceived;
            MaxHealReceived = maxHealReceived;
            TotalStealCount = totalStealCount;
            TotalMonsterHouseEnterCount = totalMonsterHouseEnterCount;
            TotalCursedItemDiscoverCount = totalCursedItemDiscoverCount;
            _itemUsedCountByBaseName = itemUsedCountByBaseName.ToSerializable();
            _deathCountByCause = deathCountByCause.ToSerializable();
            HasShownFirstGameTutorial = hasShownFirstGameTutorial;
            HasShownShopTutorial = hasShownShopTutorial;
            HasShownMagicCircleTutorial = hasShownMagicCircleTutorial;
            HasShownFloor30Tutorial = hasShownFloor30Tutorial;
        }
    }
}