#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using ObservableCollections;
using R3;
using Unity.Logging;
using Utilities;

namespace Game
{
    public class WorldStatistics : ISerializable<StatisticsMemento>, IDisposable
    {
        // プレイ・進行
        private TimeSpan _lastSavePlayTime;
        private DateTime _stateChangedAt;
        private readonly ReadOnlyReactiveProperty<GameState> _state;
        private TimeSpan CurrentSessionTime => _state.CurrentValue == GameState.Dungeon
            ? DateTime.Now - _stateChangedAt
            : TimeSpan.Zero;
        public TimeSpan PlayTime => _lastSavePlayTime + CurrentSessionTime;
        private readonly ReactiveProperty<int> _turn;
        public ReadOnlyReactiveProperty<int> Turn => _turn;
        public int MaxMapLevel { get; private set; }
        public bool IsCheating { get; set; }

        // 戦闘・ダメージ
        private int _totalDamageReceived;
        private int _maxDamageReceived;
        private int _totalDamageDealt;
        private int _maxDamageDealt;
        private int _totalHealReceived;
        private int _maxHealReceived;

        // 敵撃破
        private readonly Dictionary<string, int> _enemyTypeKilledCount = new();

        // アイテム使用
        private readonly Dictionary<string, int> _itemUsedCountByBaseName = new();

        // 死亡
        private readonly Dictionary<DeathRecord, int> _deathCounts = new();

        // 盗み（ラン内）
        public int StealCount { get; private set; }

        // モンスターハウス（1ハウスにつき1回）
        public int MonsterHouseEnterCount { get; private set; }

        // 呪われたアイテムを発見（呪いかつ識別済みになったアイテムIDをラン内で一意に記録）
        private readonly HashSet<Id<IItem>> _discoveredCursedItemIds = new();
        public int CursedItemDiscoverCount => _discoveredCursedItemIds.Count;
        private readonly GlobalStatistics _globalStatistics;
        private readonly CompositeDisposable _disposables = new();

        public WorldStatistics(StatisticsMemento memento, GameManager game, World world, GlobalStatistics globalStatistics)
        {
            _state = game.State;
            _lastSavePlayTime = TimeSpan.FromTicks(memento.PlayTime);
            _stateChangedAt = DateTime.Now;
            _turn = new(memento.Turn);
            MaxMapLevel = memento.MaxMapLevel;
            IsCheating = memento.IsCheating;
            _enemyTypeKilledCount = new Dictionary<string, int>(memento.EnemyTypeKilledCount);
            _totalDamageReceived = memento.TotalDamageReceived;
            _maxDamageReceived = memento.MaxDamageReceived;
            _totalDamageDealt = memento.TotalDamageDealt;
            _maxDamageDealt = memento.MaxDamageDealt;
            _totalHealReceived = memento.TotalHealReceived;
            _maxHealReceived = memento.MaxHealReceived;
            StealCount = memento.StealCount;
            MonsterHouseEnterCount = memento.MonsterHouseEnterCount;
            _globalStatistics = globalStatistics;
            foreach (var id in memento.DiscoveredCursedItemIds)
                _discoveredCursedItemIds.Add(id);
            foreach (var kvp in memento.ItemUsedCountByBaseName)
                _itemUsedCountByBaseName[kvp.Key] = kvp.Value;
            foreach (var kvp in memento.DeathCounts)
                _deathCounts[kvp.Key] = kvp.Value;

            var events = world.Events.OnRecorded;
            events.OfType<WorldEvent, MapEntered>()
                .Subscribe(entered => UpdateMaxMapLevel(entered.Depth)).AddTo(_disposables);
            events.OfType<WorldEvent, TurnPassed>()
                .Subscribe(_ => RecordTurn()).AddTo(_disposables);
            events.OfType<WorldEvent, ItemIdentified>()
                .Subscribe(identified => _globalStatistics.RecordKnownItem(identified.BaseName)).AddTo(_disposables);
            events.OfType<WorldEvent, IInventoryEvent>()
                .Select(inventoryEvent => inventoryEvent.Inventory)
                .Select(inventory => inventory?.Contents)
                .Where(contents => contents != null)
                .Subscribe(contents => RecordCursedItems(contents!)).AddTo(_disposables);
            events.OfType<WorldEvent, CharacterDamaged>()
                .Where(damaged => damaged.Label.IsPlayer)
                .Subscribe(damaged => RecordDamageReceived(damaged.Amount)).AddTo(_disposables);
            events.OfType<WorldEvent, CharacterDamaged>()
                .Where(damaged => damaged.Label.Affiliation == AffiliationType.Enemy && damaged.Attacker is { IsPlayer: true })
                .Subscribe(damaged => RecordDamageDealt(damaged.Amount)).AddTo(_disposables);
            events.OfType<WorldEvent, CharacterHealed>()
                .Where(healed => healed.Label.IsPlayer)
                .Subscribe(healed => RecordHealReceived(healed.Amount)).AddTo(_disposables);
            events.OfType<WorldEvent, MonsterHouseEntered>()
                .Subscribe(_ => RecordMonsterHouseEntered()).AddTo(_disposables);
            events.OfType<WorldEvent, TheftDetected>()
                .Subscribe(_ => RecordSteal()).AddTo(_disposables);
            events.OfType<WorldEvent, SkillUsed>()
                .Select(used => used.Source)
                .OfType<SkillSource, ItemSkillSource>()
                .Where(item => item.Label.IsPlayer)
                .Subscribe(item => RecordItemUsed(item.ItemBaseName)).AddTo(_disposables);
            events.OfType<WorldEvent, CharacterDied>()
                .Where(died => died.Label.Affiliation == AffiliationType.Enemy)
                .Subscribe(died => RecordEnemyKilled(died.Label.Name)).AddTo(_disposables);
            events.OfType<WorldEvent, CharacterDied>()
                .Where(died => died.Label.IsPlayer)
                .Subscribe(died => RecordDeath(new DeathRecord(died.Label, died.Source))).AddTo(_disposables);
            events.OfType<WorldEvent, CharacterBroken>()
                .Where(broken => broken.Label.Affiliation == AffiliationType.Enemy)
                .Subscribe(broken => RecordEnemyKilled(broken.Label.Name)).AddTo(_disposables);
            events.OfType<WorldEvent, CharacterBroken>()
                .Where(broken => broken.Label.IsPlayer)
                .Subscribe(broken => RecordDeath(new DeathRecord(broken.Label, new DamageSource(DamageCause.Break))))
                .AddTo(_disposables);


            _state.Pairwise().Subscribe(state =>
            {
                if (state.Previous == GameState.Dungeon)
                {
                    _lastSavePlayTime += DateTime.Now - _stateChangedAt;
                }
                _stateChangedAt = DateTime.Now;
            }).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void UpdateMaxMapLevel(int depth)
        {
            if (depth <= MaxMapLevel)
                return;
            MaxMapLevel = depth;
            _globalStatistics.RecordMaxMapLevel(depth);
        }

        private void RecordTurn()
        {
            _turn.Value++;
            _globalStatistics.RecordTurn();
        }

        private void RecordDamageReceived(int damage)
        {
            _totalDamageReceived += damage;
            if (damage > _maxDamageReceived)
                _maxDamageReceived = damage;
            _globalStatistics.RecordDamageReceived(damage);
        }

        private void RecordDamageDealt(int damage)
        {
            _totalDamageDealt += damage;
            if (damage > _maxDamageDealt)
                _maxDamageDealt = damage;
            _globalStatistics.RecordDamageDealt(damage);
        }

        private void RecordHealReceived(int amount)
        {
            _totalHealReceived += amount;
            if (amount > _maxHealReceived)
                _maxHealReceived = amount;
            _globalStatistics.RecordHealReceived(amount);
        }

        private void RecordItemUsed(string baseName)
        {
            _itemUsedCountByBaseName.TryGetValue(baseName, out var count);
            _itemUsedCountByBaseName[baseName] = count + 1;
            _globalStatistics.RecordItemUsed(baseName);
        }

        private void RecordDeath(DeathRecord death)
        {
            _deathCounts.TryGetValue(death, out var count);
            _deathCounts[death] = count + 1;
            _globalStatistics.RecordDeath(death);
        }

        private void RecordEnemyKilled(string enemyName)
        {
            _enemyTypeKilledCount.TryGetValue(enemyName, out var count);
            _enemyTypeKilledCount[enemyName] = count + 1;
            _globalStatistics.RecordEnemyKilled(enemyName);
        }

        private void RecordSteal()
        {
            StealCount++;
            _globalStatistics.RecordSteal();
        }

        private void RecordMonsterHouseEntered()
        {
            MonsterHouseEnterCount++;
            _globalStatistics.RecordMonsterHouseEntered();
        }

        private void RecordCursedItems(InventoryContents contents)
        {
            // TODO: 引き継ぎアイテム実装時は、ラン外から渡されたアイテムは記録しない
            foreach (var item in contents.Rows.Select(row => row.Item))
            {
                if (!item.IsCursed || !item.IsCurseIdentified)
                    continue;
                if (_discoveredCursedItemIds.Add(item.Id))
                    _globalStatistics.RecordCursedItemDiscovery();
            }
        }

        public StatisticsMemento Serialize()
        {
            return new StatisticsMemento(PlayTime.Ticks, _turn.Value, MaxMapLevel, IsCheating,
                _totalDamageReceived, _maxDamageReceived, _totalDamageDealt, _maxDamageDealt,
                _totalHealReceived, _maxHealReceived, StealCount, MonsterHouseEnterCount,
                _discoveredCursedItemIds,
                _itemUsedCountByBaseName, _deathCounts, _enemyTypeKilledCount);
        }

        public static StatisticsMemento Build()
        {
            return new StatisticsMemento(0, 0, 1, false, 0, 0, 0, 0, 0, 0, 0, 0,
                new HashSet<Id<IItem>>(),
                new Dictionary<string, int>(), new Dictionary<DeathRecord, int>(), new Dictionary<string, int>());
        }

        public WorldStatisticsSummary Summarize()
        {
            var common = new StatisticsSummary(
                PlayTime,
                Turn.CurrentValue,
                MaxMapLevel,
                _totalDamageReceived,
                _maxDamageReceived,
                _totalDamageDealt,
                _maxDamageDealt,
                _totalHealReceived,
                _maxHealReceived,
                StealCount,
                MonsterHouseEnterCount,
                CursedItemDiscoverCount,
                new Dictionary<string, int>(_enemyTypeKilledCount),
                new Dictionary<string, int>(_itemUsedCountByBaseName),
                new Dictionary<DeathRecord, int>(_deathCounts));
            return new WorldStatisticsSummary(common, IsCheating);
        }
    }
}
