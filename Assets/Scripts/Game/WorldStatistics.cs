#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Item;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using Unity.Logging;
using Utilities;

namespace Game
{
    public class WorldStatistics : ISerializable<StatisticsMemento>
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
        private readonly Dictionary<string, int> _deathCountByCause = new();

        public int StealCount { get; private set; }

        public int MonsterHouseEnterCount { get; private set; }

        private readonly HashSet<Id<IItem>> _discoveredCursedItemIds = new();
        public int CursedItemDiscoverCount => _discoveredCursedItemIds.Count;
        private readonly GlobalStatistics _globalStatistics;
        private readonly CompositeDisposable _mapDisposables = new();

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
            foreach (var kvp in memento.DeathCountByCause)
                _deathCountByCause[kvp.Key] = kvp.Value;

            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                _mapDisposables.Clear();
                var map = mapChanged.Map;
                UpdateMaxMapLevel(map.Depth);
                map.Player.Character.KnownItemNames.ObserveAdd().Subscribe(item =>
                    _globalStatistics.RecordKnownItem(item.Value)).AddTo(_mapDisposables);
                map.Characters.SubscribeIncludingCurrentObservables(
                    character => character.Status.OnDamageReceived,
                    (character, msg) =>
                    {
                        if (character.IsPlayer)
                            RecordDamageReceived(msg.Damage);
                        else if (map.Player.Character.Affiliation.IsEnemy(character.Affiliation) &&
                                 msg.Attacker?.IsPlayer == true)
                            RecordDamageDealt(msg.Damage);
                    }
                ).AddTo(_mapDisposables);
                map.Characters.SubscribeIncludingCurrentObservables(
                    character => character.Status.OnHealReceived,
                    (character, amount) =>
                    {
                        if (character.IsPlayer)
                            RecordHealReceived(amount);
                    }
                ).AddTo(_mapDisposables);
                map.Player.Character.OnItemUsed.Subscribe(RecordItemUsed).AddTo(_mapDisposables);
                map.Player.Character.Entity.OnDestroyed.Subscribe(cause =>
                    RecordDeath(map.Player.Character.GetNameIgnoreVisibility(map.Player) + cause)).AddTo(_mapDisposables);
                map.Characters.SubscribeIncludingCurrentObservables(
                    character => character.OnDead,
                    (character, _) =>
                    {
                        if (!character.IsPlayer && map.Player.Character.Affiliation.IsEnemy(character.Affiliation))
                            RecordEnemyKilled(character.Name);
                    }
                ).AddTo(_mapDisposables);
                map.Shop?.IsStolen.Pairwise().Subscribe(pair =>
                {
                    if (pair.Current && !pair.Previous)
                        RecordSteal();
                }).AddTo(_mapDisposables);
                map.MonsterHouse?.HasEverEntered.Pairwise().Subscribe(pair =>
                {
                    if (pair.Current && !pair.Previous)
                        RecordMonsterHouseEntered();
                }).AddTo(_mapDisposables);
                var inventory = map.Player.Character.Inventory;
                foreach (var item in inventory.AllItems)
                {
                    SubscribeCursedItems(item);
                }
                inventory.OnItemInserted.Subscribe(inserted =>
                {
                    var item = inserted.NewItem;
                    SubscribeCursedItems(item);
                }).AddTo(_mapDisposables);
            });

            game.OnTurnChanged.Subscribe(_ => RecordTurn());

            _state.Pairwise().Subscribe(state =>
            {
                if (state.Previous == GameState.Dungeon)
                {
                    _lastSavePlayTime += DateTime.Now - _stateChangedAt;
                }
                _stateChangedAt = DateTime.Now;
            });
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

        private void RecordDeath(string cause)
        {
            _deathCountByCause.TryGetValue(cause, out var count);
            _deathCountByCause[cause] = count + 1;
            _globalStatistics.RecordDeath(cause);
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

        private void SubscribeCursedItems(IItem item)
        {
            TryRecordCursedItemDiscovery(item);
            item.CurseIdentified.Subscribe(_ => TryRecordCursedItemDiscovery(item)).AddTo(_mapDisposables);
            item.Cursed.Subscribe(_ => TryRecordCursedItemDiscovery(item)).AddTo(_mapDisposables);
        }

        private void TryRecordCursedItemDiscovery(IItem item)
        {
            if (!item.IsCursed || !item.IsCurseIdentified)
                return;
            if (!_discoveredCursedItemIds.Add(item.Id))
                return;
            _globalStatistics.RecordCursedItemDiscovery();
        }

        public StatisticsMemento Serialize()
        {
            return new StatisticsMemento(PlayTime.Ticks, _turn.Value, MaxMapLevel, IsCheating,
                _totalDamageReceived, _maxDamageReceived, _totalDamageDealt, _maxDamageDealt,
                _totalHealReceived, _maxHealReceived, StealCount, MonsterHouseEnterCount,
                _discoveredCursedItemIds,
                _itemUsedCountByBaseName, _deathCountByCause, _enemyTypeKilledCount);
        }

        public static StatisticsMemento Build()
        {
            return new StatisticsMemento(0, 0, 1, false, 0, 0, 0, 0, 0, 0, 0, 0,
                new HashSet<Id<IItem>>(),
                new Dictionary<string, int>(), new Dictionary<string, int>(), new Dictionary<string, int>());
        }

        public string GetStatisticsText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== World Statistics ===");
            sb.AppendLine("--- プレイ・進行 ---");
            sb.AppendLine($"PlayTime: {PlayTime}");
            sb.AppendLine($"Turn: {Turn.CurrentValue}");
            sb.AppendLine($"MaxMapLevel: {MaxMapLevel}");
            sb.AppendLine($"IsCheating: {IsCheating}");
            sb.AppendLine("--- 戦闘・ダメージ ---");
            sb.AppendLine($"TotalDamageReceived: {_totalDamageReceived} (Max: {_maxDamageReceived})");
            sb.AppendLine($"TotalDamageDealt: {_totalDamageDealt} (Max: {_maxDamageDealt})");
            sb.AppendLine($"TotalHealReceived: {_totalHealReceived} (Max: {_maxHealReceived})");
            sb.AppendLine("--- 盗み ---");
            sb.AppendLine($"盗み回数: {StealCount}");
            sb.AppendLine("--- モンスターハウス ---");
            sb.AppendLine($"進入回数: {MonsterHouseEnterCount}");
            sb.AppendLine("--- 呪い ---");
            sb.AppendLine($"呪われたアイテムを発見した回数: {CursedItemDiscoverCount}");
            sb.AppendLine("--- 敵撃破 ---");
            sb.AppendLine($"EnemyKilledCount: {_enemyTypeKilledCount.Values.Sum()}");
            foreach (var kvp in _enemyTypeKilledCount.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            sb.AppendLine("--- アイテム使用 ---");
            sb.AppendLine($"TotalItemUsedCount: {_itemUsedCountByBaseName.Values.Sum()}");
            foreach (var kvp in _itemUsedCountByBaseName.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            sb.AppendLine("--- 死亡 ---");
            sb.AppendLine($"TotalDeathCount: {_deathCountByCause.Values.Sum()}");
            foreach (var kvp in _deathCountByCause.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            return sb.ToString();
        }
    }
}
