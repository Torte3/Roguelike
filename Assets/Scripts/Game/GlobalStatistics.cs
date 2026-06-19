#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Model;
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
        private readonly Dictionary<string, int> _deathCountByCause = new();

        public int TotalStealCount { get; private set; }

        public int TotalMonsterHouseEnterCount { get; private set; }

        public int TotalCursedItemDiscoverCount { get; private set; }

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
            foreach (var kvp in memento.DeathCountByCause)
                _deathCountByCause[kvp.Key] = kvp.Value;
        }

        public void RecordTurn() => _totalTurns.Value++;

        public void RecordMaxMapLevel(int depth)
        {
            if (depth > MaxMapLevel)
                MaxMapLevel = depth;
        }

        public void RecordDamageReceived(int damage)
        {
            _totalDamageReceived += damage;
            if (damage > _maxDamageReceived)
                _maxDamageReceived = damage;
        }

        public void RecordDamageDealt(int damage)
        {
            _totalDamageDealt += damage;
            if (damage > _maxDamageDealt)
                _maxDamageDealt = damage;
        }

        public void RecordHealReceived(int amount)
        {
            _totalHealReceived += amount;
            if (amount > _maxHealReceived)
                _maxHealReceived = amount;
        }

        public void RecordItemUsed(string baseName)
        {
            _itemUsedCountByBaseName.TryGetValue(baseName, out var count);
            _itemUsedCountByBaseName[baseName] = count + 1;
        }

        public void RecordDeath(string cause)
        {
            _deathCountByCause.TryGetValue(cause, out var count);
            _deathCountByCause[cause] = count + 1;
        }

        public void RecordEnemyKilled(string enemyName)
        {
            _enemyTypeKilledCount.TryGetValue(enemyName, out var count);
            _enemyTypeKilledCount[enemyName] = count + 1;
        }

        public void RecordSteal() => TotalStealCount++;

        public void RecordMonsterHouseEntered() => TotalMonsterHouseEnterCount++;

        public void RecordCursedItemDiscovery() => TotalCursedItemDiscoverCount++;

        public bool HasShownTutorial(TutorialType type) => type switch
        {
            TutorialType.FirstGame => HasShownFirstGameTutorial,
            TutorialType.Shop => HasShownShopTutorial,
            TutorialType.MagicCircle => HasShownMagicCircleTutorial,
            TutorialType.Floor30 => HasShownFloor30Tutorial,
            _ => false,
        };

        public void RecordTutorialShown(TutorialType type)
        {
            switch (type)
            {
                case TutorialType.FirstGame: HasShownFirstGameTutorial = true; break;
                case TutorialType.Shop: HasShownShopTutorial = true; break;
                case TutorialType.MagicCircle: HasShownMagicCircleTutorial = true; break;
                case TutorialType.Floor30: HasShownFloor30Tutorial = true; break;
            }
        }

        public void RecordKnownItem(string baseName) => _knownItemNames.Add(baseName);

        public GlobalStatisticsMemento Serialize()
        {
            return new GlobalStatisticsMemento(MaxMapLevel, _knownItemNames.ToList(), _enemyTypeKilledCount,
                TotalPlayTime.Ticks, TotalTurns.CurrentValue,
                _totalDamageReceived, _maxDamageReceived, _totalDamageDealt, _maxDamageDealt,
                _totalHealReceived, _maxHealReceived, TotalStealCount, TotalMonsterHouseEnterCount,
                TotalCursedItemDiscoverCount, _itemUsedCountByBaseName, _deathCountByCause,
                HasShownFirstGameTutorial, HasShownShopTutorial, HasShownMagicCircleTutorial, HasShownFloor30Tutorial);
        }

        public static GlobalStatisticsMemento Build()
        {
            return new GlobalStatisticsMemento(1, new(), new Dictionary<string, int>(), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                new Dictionary<string, int>(), new Dictionary<string, int>(), false, false, false, false);
        }

        public string GetStatisticsText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Global Statistics ===");
            sb.AppendLine("--- プレイ・進行 ---");
            sb.AppendLine($"TotalPlayTime: {TotalPlayTime}");
            sb.AppendLine($"TotalTurns: {TotalTurns.CurrentValue}");
            sb.AppendLine($"MaxMapLevel: {MaxMapLevel}");
            sb.AppendLine("--- 戦闘・ダメージ ---");
            sb.AppendLine($"TotalDamageReceived: {_totalDamageReceived} (Max: {_maxDamageReceived})");
            sb.AppendLine($"TotalDamageDealt: {_totalDamageDealt} (Max: {_maxDamageDealt})");
            sb.AppendLine($"TotalHealReceived: {_totalHealReceived} (Max: {_maxHealReceived})");
            sb.AppendLine("--- 盗み ---");
            sb.AppendLine($"通算盗み回数: {TotalStealCount}");
            sb.AppendLine("--- モンスターハウス ---");
            sb.AppendLine($"通算進入回数: {TotalMonsterHouseEnterCount}");
            sb.AppendLine("--- 呪い ---");
            sb.AppendLine($"呪われたアイテムを発見した回数: {TotalCursedItemDiscoverCount}");
            sb.AppendLine("--- 敵撃破 ---");
            sb.AppendLine($"EnemyKilledCount: {_enemyTypeKilledCount.Values.Sum()}");
            foreach (var kvp in _enemyTypeKilledCount.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            sb.AppendLine("--- アイテム ---");
            sb.AppendLine($"TotalItemUsedCount: {_itemUsedCountByBaseName.Values.Sum()}");
            foreach (var kvp in _itemUsedCountByBaseName.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            sb.AppendLine($"KnownItemNames Count: {_knownItemNames.Count}");
            foreach (var itemName in _knownItemNames.OrderBy(x => x))
                sb.AppendLine($"  {itemName}");
            sb.AppendLine("--- 死亡 ---");
            sb.AppendLine($"TotalDeathCount: {_deathCountByCause.Values.Sum()}");
            foreach (var kvp in _deathCountByCause.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            return sb.ToString();
        }
    }
}
