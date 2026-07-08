#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Entity;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using UnityEngine;

namespace Game
{
    public class GlobalStatistics
    {
        // プレイ・進行
        public int MaxMapLevel { get; private set; }
        private TimeSpan _lastSaveTotalPlayTime;
        private DateTime _sessionStartTime;
        private TimeSpan CurrentSessionTime => DateTime.Now - _sessionStartTime;
        public TimeSpan TotalPlayTime => _lastSaveTotalPlayTime + CurrentSessionTime;
        private readonly ReactiveProperty<int> _totalTurns;
        public ReadOnlyReactiveProperty<int> TotalTurns => _totalTurns;

        // 戦闘・ダメージ
        private int _totalDamageReceived;
        private int _maxDamageReceived;
        private int _totalDamageDealt;
        public int TotalDamageDealt => _totalDamageDealt;
        private int _maxDamageDealt;
        private int _totalHealReceived;
        private int _maxHealReceived;

        // 敵撃破
        private readonly Dictionary<string, int> _enemyTypeKilledCount = new();

        // アイテム
        private readonly ObservableHashSet<string> _knownItemNames;
        public IObservableCollection<string> KnownItemNames => _knownItemNames;
        private readonly Dictionary<string, int> _itemUsedCountByBaseName = new();

        // 死亡
        private readonly Dictionary<DeathRecord, int> _deathCounts = new();

        // 盗み（通算）
        public int TotalStealCount { get; private set; }

        // モンスターハウス（1ハウスにつき1回）
        public int TotalMonsterHouseEnterCount { get; private set; }

        // 呪われたアイテムを発見
        public int TotalCursedItemDiscoverCount { get; private set; }

        // 各チュートリアルを表示済みか（種類ごとに個別フラグ。初回のみ表示するための記録）。
        public bool HasShownFirstGameTutorial { get; private set; }
        public bool HasShownShopTutorial { get; private set; }
        public bool HasShownMagicCircleTutorial { get; private set; }
        public bool HasShownFloor30Tutorial { get; private set; }

        public GlobalStatistics(GlobalStatisticsMemento memento)
        {
            _knownItemNames = new(memento.KnownItemNames);
            _enemyTypeKilledCount = new Dictionary<string, int>(memento.EnemyTypeKilledCount);
            _lastSaveTotalPlayTime = TimeSpan.FromTicks(memento.TotalPlayTime);
            _sessionStartTime = DateTime.Now;
            _totalTurns = new ReactiveProperty<int>(memento.TotalTurns);
            MaxMapLevel = memento.MaxMapLevel;
            _totalDamageReceived = memento.TotalDamageReceived;
            _maxDamageReceived = memento.MaxDamageReceived;
            _totalDamageDealt = memento.TotalDamageDealt;
            _maxDamageDealt = memento.MaxDamageDealt;
            _totalHealReceived = memento.TotalHealReceived;
            _maxHealReceived = memento.MaxHealReceived;
            TotalStealCount = memento.TotalStealCount;
            TotalMonsterHouseEnterCount = memento.TotalMonsterHouseEnterCount;
            TotalCursedItemDiscoverCount = memento.TotalCursedItemDiscoverCount;
            HasShownFirstGameTutorial = memento.HasShownFirstGameTutorial;
            HasShownShopTutorial = memento.HasShownShopTutorial;
            HasShownMagicCircleTutorial = memento.HasShownMagicCircleTutorial;
            HasShownFloor30Tutorial = memento.HasShownFloor30Tutorial;
            foreach (var kvp in memento.ItemUsedCountByBaseName)
                _itemUsedCountByBaseName[kvp.Key] = kvp.Value;
            foreach (var kvp in memento.DeathCounts)
                _deathCounts[kvp.Key] = kvp.Value;
        }

        internal void RecordTurn() => _totalTurns.Value++;

        internal void RecordMaxMapLevel(int depth)
        {
            if (depth > MaxMapLevel)
                MaxMapLevel = depth;
        }

        internal void RecordDamageReceived(int damage)
        {
            _totalDamageReceived += damage;
            if (damage > _maxDamageReceived)
                _maxDamageReceived = damage;
        }

        internal void RecordDamageDealt(int damage)
        {
            _totalDamageDealt += damage;
            if (damage > _maxDamageDealt)
                _maxDamageDealt = damage;
        }

        internal void RecordHealReceived(int amount)
        {
            _totalHealReceived += amount;
            if (amount > _maxHealReceived)
                _maxHealReceived = amount;
        }

        internal void RecordItemUsed(string baseName)
        {
            _itemUsedCountByBaseName.TryGetValue(baseName, out var count);
            _itemUsedCountByBaseName[baseName] = count + 1;
        }

        internal void RecordDeath(DeathRecord death)
        {
            _deathCounts.TryGetValue(death, out var count);
            _deathCounts[death] = count + 1;
        }

        internal void RecordEnemyKilled(string enemyName)
        {
            _enemyTypeKilledCount.TryGetValue(enemyName, out var count);
            _enemyTypeKilledCount[enemyName] = count + 1;
        }

        internal void RecordSteal() => TotalStealCount++;

        internal void RecordMonsterHouseEntered() => TotalMonsterHouseEnterCount++;

        internal void RecordCursedItemDiscovery() => TotalCursedItemDiscoverCount++;

        internal bool HasShownTutorial(TutorialType type) => type switch
        {
            TutorialType.FirstGame => HasShownFirstGameTutorial,
            TutorialType.Shop => HasShownShopTutorial,
            TutorialType.MagicCircle => HasShownMagicCircleTutorial,
            TutorialType.Floor30 => HasShownFloor30Tutorial,
            _ => false,
        };

        internal void RecordTutorialShown(TutorialType type)
        {
            switch (type)
            {
                case TutorialType.FirstGame: HasShownFirstGameTutorial = true; break;
                case TutorialType.Shop: HasShownShopTutorial = true; break;
                case TutorialType.MagicCircle: HasShownMagicCircleTutorial = true; break;
                case TutorialType.Floor30: HasShownFloor30Tutorial = true; break;
            }
        }

        internal void RecordKnownItem(string baseName) => _knownItemNames.Add(baseName);

        public GlobalStatisticsMemento Serialize()
        {
            return new GlobalStatisticsMemento(MaxMapLevel, _knownItemNames.ToList(), _enemyTypeKilledCount,
                TotalPlayTime.Ticks, TotalTurns.CurrentValue,
                _totalDamageReceived, _maxDamageReceived, _totalDamageDealt, _maxDamageDealt,
                _totalHealReceived, _maxHealReceived, TotalStealCount, TotalMonsterHouseEnterCount,
                TotalCursedItemDiscoverCount, _itemUsedCountByBaseName, _deathCounts,
                HasShownFirstGameTutorial, HasShownShopTutorial, HasShownMagicCircleTutorial, HasShownFloor30Tutorial);
        }

        public static GlobalStatisticsMemento Build()
        {
            return new GlobalStatisticsMemento(1, new(), new Dictionary<string, int>(), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                new Dictionary<string, int>(), new Dictionary<DeathRecord, int>(), false, false, false, false);
        }

        public GlobalStatisticsSummary Summarize()
        {
            var common = new StatisticsSummary(
                TotalPlayTime,
                TotalTurns.CurrentValue,
                MaxMapLevel,
                _totalDamageReceived,
                _maxDamageReceived,
                _totalDamageDealt,
                _maxDamageDealt,
                _totalHealReceived,
                _maxHealReceived,
                TotalStealCount,
                TotalMonsterHouseEnterCount,
                TotalCursedItemDiscoverCount,
                new Dictionary<string, int>(_enemyTypeKilledCount),
                new Dictionary<string, int>(_itemUsedCountByBaseName),
                new Dictionary<DeathRecord, int>(_deathCounts));
            return new GlobalStatisticsSummary(common, _knownItemNames.ToList());
        }
    }
}
