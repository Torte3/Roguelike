#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Configuration;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Message;
using Domain.Model.Character.Status;
using Domain.Model.Character.Type;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using Domain.Service.Action;
using Domain.Service.Characters.Behavior;
using Domain.Service.Characters.Conditions;
using Domain.Service.Effect;
using Domain.Service.Items;
using ObservableCollections;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Characters
{
    internal sealed class Character : ICharacter, IReadOnlyPlayerCharacter
    {
        private readonly string _name;
        private readonly CharacterAffiliationManager _affiliationManager;
        private readonly Aggression _aggression;
        private readonly ReactiveProperty<Direction8> _direction;
        public EntityBase Entity { get; init; }
        private readonly Inventory _inventory;
        private readonly ObservableHashSet<string> _knownItemNames = new();
        private readonly List<CharacterSkillWithRule> _skills;
        private readonly SpawnEffectSkill? _lastSkill;
        private readonly CharacterStatusManager _statusManager;
        private readonly ObservableList<IPlayerEvent> _events = new();
        private IMap _map;
        private readonly KnownTerrain _knownTerrain;
        private Option<IAction> _chargeAction = Option.None<IAction>();
        private EffectArea? _chargeArea;
        private Option<Vector2Int> _chargeStartPosition = Option.None<Vector2Int>();
        private int _chargeTurn;
        private IDisposable? _chargePositionCancelSubscription;
        private readonly CompositeDisposable _disposables = new();
        private readonly Subject<DeathRecord> _onPerished = new();

        internal Character(CharacterMemento data, ICharacterBehavior behavior, IMap map, bool isPlayer)
        {
            IsPlayer = isPlayer;
            _name = data.Name;
            CharacterType = data.CharacterType;
            Entity = new EntityBase(data.Entity);
            _direction = new ReactiveProperty<Direction8>(data.Direction);
            _statusManager = new CharacterStatusManager(data.Status, Entity.Position, this, map);
            _skills = data.Skills.Select(x => new CharacterSkillWithRule(x)).ToList();
            _lastSkill = data.LastSkill.HasValue ? new SpawnEffectSkill(data.LastSkill.Value) : null;
            _inventory = new Inventory(data.Inventory, this);
            _knownItemNames = new ObservableHashSet<string>(data.KnownItemNames);
            _behavior = behavior;
            _canThroughWalls = data.CanThroughWalls;
            _affiliationManager = new CharacterAffiliationManager(Entity.Id, data.Affiliation, map.Player,
                affiliation => Entity.Record(new AffiliationChanged(Entity.Ref, affiliation, ((IEntity)this).AppearsInteractable(_map))));
            _aggression = data.Aggression;
            IsLeader = data.IsLeader;
            IsShiny = data.IsShiny;
            IsBoss = data.IsBoss;
            IsFlying = data.IsFlying;
            CanPickUp = data.CanPickUp;
            CanUseItem = data.CanUseItem;
            CanReceivePlayerGift = data.CanReceivePlayerGift;

            _map = map;
            _knownTerrain = new KnownTerrain(data.TerrainMemory, map);

            HasEvent = _events.ObserveCountChanged().Select(x => x > 0).ToReadOnlyReactiveProperty();

            AutoIdentify.SkipLatestValueOnSubscribe().Subscribe(autoIdentify =>
            {
                if (autoIdentify)
                {
                    foreach (var item in Inventory.AllItems)
                    {
                        KnowItem(item, false);
                    }
                }
            }).AddTo(_disposables);

            CurseAutoIdentify.SkipLatestValueOnSubscribe().Subscribe(curseAutoIdentify =>
            {
                if (curseAutoIdentify)
                {
                    foreach (var item in Inventory.AllItems)
                    {
                        KnowCurse(item, false);
                    }
                }
            }).AddTo(_disposables);

            VisionRange.OnVisibleAreaChanged.Subscribe(_ => _knownTerrain.Update(VisionRange)).AddTo(_disposables);

            _chargePositionCancelSubscription = Entity.Position.Skip(1).Subscribe(_ =>
                {
                    if (_chargeAction.HasValue
                        && _chargeStartPosition.IsSome(out var start)
                        && start != Entity.CurrentPosition)
                        ResetChargeAction();
                }).AddTo(_disposables);
        }

        public Location CurrentLocation => new(_map.Id, Entity.CurrentPosition);
        public bool IsDead => _statusManager.IsDead || Entity.IsDestroyed;
        private ICharacterBehavior _behavior { get; }
        public IPassableChecker KnownTerrain => _knownTerrain;
        public string Name => _name;
        public bool IsPlayer { get; init; }
        public bool IsLeader { get; init; }
        public bool IsShiny { get; init; }
        public bool IsBoss { get; init; }
        public bool IsFlying { get; init; }
        public bool IsGrounded => !IsFlying;
        public bool _canThroughWalls { get; init; }
        public bool CanThroughWalls => _canThroughWalls ? true : IsPlayer && Settings.WorldSettings.IgnoreWall.CurrentValue;
        public bool CanPickUp { get; init; }
        public bool CanUseItem { get; init; }
        public bool CanReceivePlayerGift { get; init; }
        public bool CanReadItem => !Status.IsFlagStat(FlagStatType.Blind);
        public ReadOnlyReactiveProperty<bool> AutoIdentify => Observable
            .CombineLatest(
                _statusManager.GetFlagProperty(FlagStatType.AutoIdentify),
                Settings.WorldSettings.AutoIdentify.Value,
                (statusFlag, worldSetting) => statusFlag || worldSetting
            ).ToReadOnlyReactiveProperty();
        public ReadOnlyReactiveProperty<bool> CurseAutoIdentify =>
            _statusManager.GetFlagProperty(FlagStatType.CurseIdentify);
        public CharacterState State { get; set; } = CharacterState.Wait;
        public IReadOnlyList<IPlayerEvent> Events => _events;
        public ReadOnlyReactiveProperty<bool> HasEvent { get; init; }

        public void SetWaitState()
        {
            State = CharacterState.Wait;
        }

        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;
        public Observable<OnStartItemSelectMessage> OnStartItemSelect => _behavior.OnStartItemSelect;
        public Observable<Unit> OnSelectedItemSelect => _behavior.OnSelectedItemSelect;
        public IObservableCollection<string> KnownItemNames => _knownItemNames;
        private ChargeState? Charge => _chargeTurn > 0 && _chargeArea != null ? new ChargeState(_chargeTurn, _chargeArea) : null;
        public CharacterLabel Label => new(_name, IsPlayer, AffiliationTowardPlayer);
        public Health Health => new(CurrentHp, CurrentMaxHp);

        private AffiliationType AffiliationTowardPlayer =>
            _affiliationManager.GetAffiliationType(_map.Player.Character.Affiliation);

        public bool CanBeBrokenBy(BreakTargets targets) => targets.HasFlag(BreakTargets.Character);

        public Observable<DeathRecord> OnPerished => _onPerished;

        public void Break(IMap map)
        {
            Entity.Record(new CharacterBroken(Entity.Ref, Label, map.ShopLookIn()));
            _onPerished.OnNext(new DeathRecord(Label, new DamageSource(DamageCause.Break)));
            Entity.Destroy();
        }

        public EntityLabel LabelIn(IMap map)
        {
            return new CharacterEntityLabel(Label);
        }

        public WorldEvent Appeared(IMap map)
        {
            return new CharacterAppeared(
                Entity.Ref,
                new Appearance(EntityKind.Character, Entity.Layer, null, IsShiny),
                new CharacterLooks(CharacterType.TypeName(), CharacterType.SubtypeName(), IsBoss, IsFlying),
                Label,
                CurrentDirection,
                Health,
                _statusManager.Particles,
                Charge,
                map.IsKeyHolder(Entity.Id),
                ((IEntity)this).AppearsInteractable(map));
        }

        public InventoryLook? HeldItemsIn(IMap map, params IReadOnlyItem[] changedItems) => this.InventoryLookIn(map, changedItems);

        public Underfoot? UnderfootAfterMove(IMap map, Vector2Int destination)
        {
            return IsPlayer ? new Underfoot(map.ItemAt(destination)?.LookIn(map)) : null;
        }

        public ICharacterType CharacterType { get; init; }
        public IStatusManager Status => _statusManager;
        IReadOnlyStatus IHasStatus.Status => _statusManager;
        IReadOnlyStatus IReadOnlyPlayerCharacter.Status => _statusManager;
        public Aggression Aggression => _aggression;
        public IAffiliation Affiliation => _affiliationManager;
        public Direction8 CurrentDirection => Direction.CurrentValue;
        public IInventory Inventory => _inventory;
        IReadOnlyStorage IReadOnlyPlayerCharacter.Inventory => _inventory;

        public EffectArea? PreviewEffectArea(ItemFocus focus)
        {
            var item = focus.GetItem(Inventory, _map);
            if (item == null || !IsKnownItem(item) || !item.SkillOnUse.IsSome(out var skill))
                return null;

            return skill.AreaOf(this, Entity.CurrentPosition, CurrentDirection, _map, true);
        }

        public Vector2Int Position => Entity.CurrentPosition;
        public IReadOnlyList<ICharacterSkillWithRule> Skills => _skills;
        public IVisionRange VisionRange => _statusManager.VisionRange;
        public IEnumerable<Vector2Int> VisibleArea => _statusManager.VisionRange.VisibleArea;

        public void RememberTerrainBefore(IReadOnlyList<(Vector2Int Position, TileData Tile)> previousTiles)
        {
            _knownTerrain.Remember(previousTiles, VisionRange);
        }

        public Route RouteTo(Vector2Int destination, IMap map)
        {
            return AStar.FindRoute(new MoveCostCalculator(this, map, true).Calculate, Entity.CurrentPosition, destination);
        }

        public bool AcceptsSwapFrom(Vector2Int requesterPosition, IMap map)
        {
            return _behavior.AcceptsSwapFrom(this, requesterPosition, map);
        }

        #region CanMove

        // CanMove 系は「そのマスへ進めるか」を判定する。引数を省いた版は、現在地（position）や
        // このキャラの能力（isFlying / canThroughWalls）を既定値として埋めて下の本体へ委譲するだけ。
        // CanMove と CanMoveIgnoreEntity の違いは、移動先のエンティティを通行の妨げと見なすか否か。

        /// <summary>
        /// その方向へ移動できるかを判定する。壁抜け能力がある場合は、移動先が通過不可でも true を返す。
        /// 「移動先の地形そのものが通過可能か」を知りたいだけなら World.IsPassable を使う。
        /// </summary>
        public bool CanMove(Vector2Int position, Direction8 direction, IPassableChecker map)
        {
            return CanMove(position, direction, IsFlying, CanThroughWalls, map);
        }

        public bool CanMove(Direction8 direction, bool isFlying, bool canThroughWalls, IPassableChecker map)
        {
            return CanMove(Entity.CurrentPosition, direction, isFlying, canThroughWalls, map);
        }

        public bool CanMove(Direction8 direction, IPassableChecker map)
        {
            return CanMove(Entity.CurrentPosition, direction, IsFlying, CanThroughWalls, map);
        }

        public bool CanMove(Vector2Int position, Direction8 direction, bool isFlying, bool canThroughWalls,
            IPassableChecker map)
        {
            if (canThroughWalls)
            {
                return map.At(position + direction.Vector())
                    .CanPlace(isFlying, canThroughWalls, false, EntityLayer.Middle);
            }

            // 斜め移動は、両隣のマスがどちらも通過可能なときだけ許可する（壁の角を斜めにすり抜けさせない）。
            return map.At(position + direction.Vector()).CanPlace(isFlying, canThroughWalls, false, EntityLayer.Middle)
                   && (!direction.IsDiagonal() ||
                       (map.At(position + direction.Rotate45Clockwise().Vector()).IsPassableOnMap() &&
                        map.At(position + direction.Rotate45AntiClockwise().Vector()).IsPassableOnMap()));
        }

        public bool CanSwap(Direction8 direction, IMap map)
        {
            return CanSwap(Entity.CurrentPosition, direction, map);
        }

        public bool CanSwap(Vector2Int position, Direction8 direction, IMap map)
        {
            var destination = position + direction.Vector();
            var target = map.GetCharacterAt(destination);
            if (target == null)
                return false;
            if (target.IsEnemy(this))
                return false;
            if (target.IsPlayer)
                return false;
            return target.CanMoveIgnoreEntity(destination, direction.Reverse(), map) &&
                   CanMoveIgnoreEntity(position, direction, map);
        }

        public bool CanMoveIgnoreEntity(Direction8 direction, IPassableChecker map)
        {
            return CanMoveIgnoreEntity(Entity.CurrentPosition, direction, map);
        }

        public bool CanMoveIgnoreEntity(Vector2Int position, Direction8 direction, IPassableChecker map)
        {
            if (CanThroughWalls)
                return map.At(position + direction.Vector())
                    .CanPlace(IsFlying, CanThroughWalls, true, EntityLayer.Middle);

            return map.At(position + direction.Vector()).CanPlace(IsFlying, CanThroughWalls, true, EntityLayer.Middle)
                   && (!direction.IsDiagonal() ||
                       (map.At(position + direction.Rotate45Clockwise().Vector()).IsPassableOnMap() &&
                        map.At(position + direction.Rotate45AntiClockwise().Vector()).IsPassableOnMap()));
        }

        #endregion

        #region Action

        public void ResetChargeAction()
        {
            var wasCharging = _chargeTurn > 0;
            _chargeAction = Option.None<IAction>();
            _chargeArea = null;
            _chargeStartPosition = Option.None<Vector2Int>();
            _chargeTurn = 0;
            if (wasCharging)
                Entity.Record(new ChargeEnded(Entity.Ref));
        }

        private void StartCharge(IAction action, ISkillWithCost skill, Direction8 direction)
        {
            _chargeAction = Option.Some(action);
            _chargeArea = skill.AreaOf(this, Entity.CurrentPosition, direction, _map, false);
            _chargeStartPosition = Option.Some(Entity.CurrentPosition);
            _chargeTurn = skill.ChargeTurn;
            Turn(direction);
            Entity.Record(new ChargeStarted(Entity.Ref, Charge));
        }

        private void CountDownCharge()
        {
            _chargeTurn--;
            Entity.Record(_chargeTurn > 0 ? new ChargeCountedDown(Entity.Ref, _chargeTurn) : new ChargeEnded(Entity.Ref));
        }

        public async UniTask DoNextAction(IGameManager gameManager, IMap map, IInput input)
        {
            State = CharacterState.Think;
            if (_chargeTurn > 0)
            {
                CountDownCharge();
            }

            if (_chargeAction.HasValue && _chargeTurn == 0)
            {
                State = CharacterState.Act;
                await _chargeAction.Value.Do(this, map);
                ResetChargeAction();
            }
            else if (_chargeTurn > 0)
            {
                DoNothing();
            }
            else
            {
                var action = await _behavior.GenerateNextAction(this, gameManager, map, input);
                if (Status.IsFlagStat(FlagStatType.Confused))
                {
                    action = RegenerateConfuseAction(map, action);
                }

                if (action is UseSkill useSkill)
                {
                    if (useSkill.Skill.Cost > 0)
                    {
                        await LoseHp(useSkill.Skill.Cost, new DamageSource(DamageCause.ItemCost), null);
                        if (IsDead)
                        {
                            DoNothing();
                            return;
                        }
                    }
                    if (useSkill.Skill.ChargeTurn > 0)
                    {
                        StartCharge(useSkill, useSkill.Skill, useSkill.Direction);
                        DoNothing();
                        return;
                    }
                }
                else if (action is UseItem useItem
                    && useItem.Item.SkillOnUse.IsSome(out var skillOnUse))
                {
                    if (skillOnUse.Cost > 0)
                    {
                        await LoseHp(skillOnUse.Cost, new DamageSource(DamageCause.ItemCost), null);
                        if (IsDead)
                        {
                            DoNothing();
                            return;
                        }
                    }
                    if (skillOnUse.ChargeTurn > 0)
                    {
                        StartCharge(useItem, skillOnUse, useItem.Direction);
                        DoNothing();
                        return;
                    }
                }

                State = CharacterState.Act;
                await action.Do(this, map);
            }
        }

        private IAction RegenerateConfuseAction(IMap map, IAction action)
        {
            switch (action)
            {
                case Move _:
                case Swap _:
                    var moves = new List<IAction>();
                    foreach (var direction in DirectionMethods.AllDirections)
                    {
                        var move = new Move(direction);
                        var swap = new Swap(direction);
                        if (move.Doable(this, map))
                            moves.Add(move);
                        else if (swap.Doable(this, map))
                            moves.Add(swap);
                    }

                    return moves.GetAtRandom();

                case UseSkill useSkill:
                    return useSkill with { Direction = DirectionMethods.AllDirections.GetAtRandom() };

                case UseItem useItem:
                    return useItem with { Direction = DirectionMethods.AllDirections.GetAtRandom() };

                case ThrowItem throwItem:
                    return throwItem with { Direction = DirectionMethods.AllDirections.GetAtRandom() };

                case DoNothing _:
                    return action;

                default:
                    throw new InvalidOperationException();
            }
        }

        public void Turn(Direction8 direction)
        {
            if (_direction.Value == direction)
                return;
            _direction.Value = direction;
            Entity.Record(new DirectionChanged(Entity.Ref, direction));
        }

        public void FaceNearestCharacter(IMap map)
        {
            var nearestCharacterDirection = map.GetVisibleCharacters(this)
                .Where(x => x != this)
                .Select(x => (character: x,
                    direction: DirectionMethods.FromVectorStrict(x.Entity.CurrentPosition - Entity.CurrentPosition)))
                .Where(x => x.direction.HasValue)
                .OrderBy(x =>
                    VectorExtension.ChebyshevDistance(x.character.Entity.CurrentPosition, Entity.CurrentPosition))
                .ThenByDescending(x => CurrentDirection.AngleTo(x.direction.Value).Value)
                .FirstOrDefault().direction;
            if (nearestCharacterDirection.HasValue)
            {
                Turn(nearestCharacterDirection.Value);
            }
        }

        public void DoNothing()
        {
            Log.Debug($"[Action]{_name}:DoNothing");
            State = CharacterState.Finish;
        }

        public void Move(Direction8 direction)
        {
            Log.Debug(
                $"[Action]{_name}:Move direction:{direction} destination:{Entity.CurrentPosition + direction.Vector()}");
            Turn(direction);
            Entity.Move(direction, MoveKind.Walk);

            State = CharacterState.Finish;
        }

        public void Teleport(Vector2Int position)
        {
            Entity.Teleport(position);

            State = CharacterState.Finish;
        }

        public async UniTask UseSkill(ISkillWithCost skill, Direction8 direction, IMap map)
        {
            Log.Debug($"[Action]{_name}:UseSkill\n{skill.Description()}\ndirection:{direction}");
            if (!_chargeAction.HasValue)
                Turn(direction);
            for (var i = 0; i < skill.RushDistance; i++)
            {
                if (CanMove(direction, map) && !_statusManager.IsFlagStat(FlagStatType.CannotMove))
                    Entity.Move(direction, MoveKind.Thrown);
            }

            if (IsDead)
            {
                State = CharacterState.Finish;
                return;
            }

            map.Events.Record(new SkillUsed(Entity.Ref, new CharacterSkillSource(Label, skill.Log)));
            await skill.Use(this, null, Entity.CurrentPosition, direction, map);

            for (var i = 0; i < skill.BackStepDistance; i++)
            {
                if (CanMove(direction.Reverse(), map) && !_statusManager.IsFlagStat(FlagStatType.CannotMove))
                    Entity.Move(direction.Reverse(), MoveKind.Thrown);
            }

            State = CharacterState.Finish;
        }

        public async UniTask UseLastSkill()
        {
            if (_lastSkill != null)
            {
                _map.Events.Record(new SkillUsed(Entity.Ref, new CharacterSkillSource(Label, _lastSkill.Log)));
                await _lastSkill.Use(this, null, Entity.CurrentPosition, CurrentDirection, _map);
            }
        }

        public async UniTask UseItem(IItem item, Direction8 direction, IMap map)
        {
            Log.Debug($"[Action]{_name}:UseItem\n{item.DebugInfo()}\ndirection:{direction}");
            if (!_chargeAction.HasValue)
                Turn(direction);

            if (item.CanActivateWhenUsed)
            {
                map.Events.Record(new SkillUsed(Entity.Ref, new ItemSkillSource(Label, item.NameIn(map), item.BaseName,
                    item.Category, item.UseKind)));

                var result = await item.SkillOnUse.Expect("skill on use is null").Skill.Match(
                    async spawnEffect =>
                    {
                        for (var i = 0; i < spawnEffect.RushDistance; i++)
                        {
                            if (CanMove(direction, map) && !_statusManager.IsFlagStat(FlagStatType.CannotMove))
                                Entity.Move(direction, MoveKind.Thrown);
                        }

                        var result = await item.Use(this, Entity.CurrentPosition, direction, map);

                        for (var i = 0; i < spawnEffect.BackStepDistance; i++)
                        {
                            if (CanMove(direction.Reverse(), map) && !_statusManager.IsFlagStat(FlagStatType.CannotMove))
                                Entity.Move(direction.Reverse(), MoveKind.Thrown);
                        }

                        return result;
                    },
                    async itemTarget => await item.Use(this, Entity.CurrentPosition, direction, map),
                    async inventoryTarget => await item.Use(this, Entity.CurrentPosition, direction, map),
                    async _ => await item.Use(this, Entity.CurrentPosition, direction, map)
                );
                if (result.Result == SkillResult.Success)
                {
                    if (item.IdentifyIfUsed)
                    {
                        KnowItem(item, true);
                    }
                }
            }
            else if (item.CanAttemptUse && item.ActivationCheckWhenUsed().IsFailed(out var failure))
            {
                map.Events.Record(new ItemActionFailed(Entity.IsVisible, item.NameIn(map), failure));
            }

            State = CharacterState.Finish;
        }

        public async UniTask UseItemOnDeath()
        {
            var items = Inventory.AllItems.Where(x => x.UseOnDeath).ToList();
            foreach (var item in items)
            {
                await UseItem(item, CurrentDirection, _map);
                if (!IsDead)
                    break;
            }
        }

        public async UniTask ThrowItem(IItem item, Direction8 direction, IMap map)
        {
            Log.Debug(
                $"[Action]{_name}:ThrowItem\n{item.DebugInfo()}\n direction:{direction}");
            Turn(direction);

            KnowCurse(item, true);
            if (item.ThrowCheck().IsFailed(out var failure))
            {
                map.Events.Record(new ItemActionFailed(Entity.IsVisible, item.NameIn(map), failure));
                State = CharacterState.Finish;
                return;
            }

            var isFromInventory = _inventory.Contains(item);
            if (isFromInventory)
            {
                _inventory.Remove(item);
            }
            else
            {
                map.TryPickUpAt(Entity.CurrentPosition, true);
            }

            var destination =
                ItemEntity.GetThrowDestination(Entity.CurrentPosition, direction, CommonSenseParameters.ThrowDistance,
                    map);

            map.Events.Record(new ItemThrown(Entity.Ref, Label, item.NameIn(map),
                new Flight(item.Icon, Entity.CurrentPosition, destination),
                isFromInventory ? HeldItemsIn(map) : null,
                isFromInventory ? null : UnderfootAfterMove(map, Entity.CurrentPosition)));

            if (item.ShouldRevealMimic(this, destination, map))
            {
                State = CharacterState.Finish;
                return;
            }

            var itemEntity = map.SpawnItem(item,
                map.FindBlankPositionFrom(destination, position => map.At(position).IsBlank(EntityLayer.Bottom)));

            item = itemEntity.Item;
            if (item.CanActivateWhenThrown)
            {
                var result = await item.UseWhenThrown(this, destination, direction, map);
            }

            State = CharacterState.Finish;
        }

        public void ForceDropItem(int index, IMap map)
        {
            if (Inventory.CanRemove(index))
            {
                var item = Inventory.Remove(index);
                map.Events.Record(new ItemDropped(Entity.Ref, Label, item.NameIn(map), HeldItemsIn(map)));
                if (!item.ShouldRevealMimic(this, Entity.CurrentPosition, map))
                {
                    map.SpawnItem(item,
                        map.FindBlankPositionFrom(Entity.CurrentPosition,
                            position => map.At(position).IsBlank(EntityLayer.Bottom)));
                }
                State = CharacterState.Finish;
            }
        }
        public void PickUpItem(IMap map)
        {
            var groundItem = map.Items.At(Entity.CurrentPosition).First();

            if (Inventory.CanAddToEmpty())
            {
                map.TryPickUpAt(Entity.CurrentPosition, true);
                Inventory.AddToEmpty(groundItem.Item);
                map.Events.Record(new ItemPickedUp(Entity.Ref, Label, groundItem.Item.NameIn(map), false,
                    new ObtainedItem(groundItem.Item.Icon, Entity.CurrentPosition), HeldItemsIn(map),
                    UnderfootAfterMove(map, Entity.CurrentPosition)));
            }
            else
            {
                throw new Exception("Can't add item to inventory");
            }

            State = CharacterState.Finish;
        }

        public void DropItem(IItem item, IMap map)
        {
            if (item.DiscardCheck().IsFailed(out var failure))
            {
                map.Events.Record(new ItemActionFailed(Entity.IsVisible, item.NameIn(map), failure));
                State = CharacterState.Finish;
                return;
            }

            var groundItem = map.Items.At(Entity.CurrentPosition).FirstOrDefault();
            var index = Inventory.GetItemIndex(item).Value;

            if (Inventory.CanReplaceOrRemove(groundItem?.Item, index))
            {
                var replacedItem = Inventory.ReplaceOrRemove(groundItem?.Item, index);
                if (groundItem != null)
                {
                    map.TryPickUpAt(Entity.CurrentPosition, true);
                    map.Events.Record(new ItemExchanged(Entity.Ref, Label, replacedItem.NameIn(map), groundItem.Item.NameIn(map),
                        new ObtainedItem(groundItem.Item.Icon, Entity.CurrentPosition), HeldItemsIn(map),
                        UnderfootAfterMove(map, Entity.CurrentPosition)));
                }
                else
                {
                    map.Events.Record(new ItemDiscarded(Entity.Ref, Label, replacedItem.NameIn(map), HeldItemsIn(map)));
                }
                if (!item.ShouldRevealMimic(this, Entity.CurrentPosition, map))
                {
                    map.SpawnItem(item,
                        map.FindBlankPositionFrom(Entity.CurrentPosition,
                            position => map.At(position).IsBlank(EntityLayer.Bottom)));
                }
            }
            else
            {
                throw new Exception("Can't replace or remove item in inventory");
            }

            State = CharacterState.Finish;
        }

        public float EvaluateThrow(IItem item, Direction8 direction, IMap map)
        {
            return ItemEntity.EvaluateThrow(item, Entity.CurrentPosition, this, direction,
                CommonSenseParameters.ThrowDistance, map);
        }

        #endregion

        public void Dispose()
        {
            _chargePositionCancelSubscription?.Dispose();
            Entity.Dispose();
            _inventory.Dispose();
            _direction.Dispose();
            _disposables.Dispose();
        }

        public CharacterMemento Serialize()
        {
            return new CharacterMemento
            (
                _name,
                CharacterType,
                _behavior.Serialize(),
                _statusManager.Serialize(),
                Entity.Serialize(),
                _direction.CurrentValue,
                _skills.Select(x => x.Serialize()).ToList(),
                _lastSkill.ToOption().Map(x => x.Serialize()),
                _inventory.Serialize(),
                _knownItemNames.ToList(),
                _affiliationManager.Serialize(),
                Aggression,
                IsLeader,
                IsShiny,
                IsBoss,
                IsFlying,
                _canThroughWalls,
                CanPickUp,
                CanUseItem,
                CanReceivePlayerGift,
                _knownTerrain.Serialize()
            );
        }

        public async UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            for (var i = 0; i < distance; i++)
            {
                if (!CanMove(direction, true, CanThroughWalls, map))
                {
                    var remaining = distance - i;
                    var next = Entity.CurrentPosition + direction.Vector();
                    if (remaining > 0 && !CanThroughWalls)
                    {
                        var mover = BlowAwayCollisionSide.FromCharacter(this);
                        var blocker = !map.At(next).IsPassableOnMap()
                            ? BlowAwayCollisionSide.Wall()
                            : BlowAwayCollisionSide.FromEntity(map.GetEntityFastAt(next, EntityLayer.Middle));
                        if (blocker.HasValue)
                        {
                            await BlowAwayCollision.Apply(mover, blocker.Value, remaining, actor as ICharacter, map);
                        }
                    }

                    break;
                }

                Entity.Move(direction, MoveKind.Thrown);
            }

            if (!map.At(Entity.CurrentPosition).CanPlace(IsFlying, CanThroughWalls, true, EntityLayer.Middle))
            {
                var position = map.FindBlankPositionFrom(Entity.CurrentPosition,
                    position => map.At(position).IsBlank(EntityLayer.Middle));
                Entity.Teleport(position);
            }
        }

        public void Die(DamageSource source)
        {
            Entity.Record(new CharacterDied(Entity.Ref, Label, source));
            if (!IsPlayer)
            {
                foreach (var item in Inventory.Clear())
                    _map.SpawnItem(item, Entity.CurrentPosition);
            }

            _onPerished.OnNext(new DeathRecord(Label, source));
            Entity.Destroy();
        }

        public void ApplyKillHealToAttacker(ICharacter? attacker)
        {
            if (attacker == null || attacker == this)
                return;
            var killer = attacker;
            if (!killer.Status.IsFlagStat(FlagStatType.KillHeal))
                return;
            if (!killer.Affiliation.IsEnemy(Affiliation))
                return;

            killer.GainHp(CommonSenseParameters.KillHealPerEnemyDefeated, HealCause.KillHeal);
        }

        #region Status

        public int CurrentMaxHp => _statusManager.Hp.Max.CurrentIntValue;
        public int CurrentHp => _statusManager.Hp.Value.CurrentValue;

        public void GainHp(int value, HealCause cause)
        {
            _statusManager.GainHp(value, cause);
        }

        public async UniTask<int> LoseHp(int value, DamageSource source, ICharacter? attacker)
        {
            return await _statusManager.LoseHp(value, source, attacker);
        }

        public void RestoreToFullHealth()
        {
            _statusManager.RestoreToFullHealth();
        }

        public void AddCondition(Id<IEntity> actor, ConditionTemplate condition)
        {
            _statusManager.AddCondition(actor, condition);
        }

        public void ClearCondition()
        {
            _statusManager.ClearCondition();
        }

        #endregion

        #region ItemKnowledge

        public void KnowItem(IItem item, bool log)
        {
            if (!IsPlayer || IsKnownItem(item))
                return;

            var unidentifiedName = item.NameIn(_map);
            _knownItemNames.Add(item.BaseName);
            _map.Events.Record(new ItemIdentified(Entity.IsVisible, unidentifiedName, item.NameIn(_map), item.BaseName, log,
                this.WholeInventoryLookIn(_map), _map.PlayerUnderfoot()));
        }

        public void KnowCurse(IItem item, bool log)
        {
            if (!IsPlayer)
                return;

            item.SetCurseIdentified(true, this, _map, !IsCurseKnown(item) && log);
        }

        public bool IsCurseKnown(IItem item) => item.IsCurseIdentified;

        public bool IsKnownItem(IReadOnlyItem item)
        {
            return _knownItemNames.Contains(item.BaseName) || Settings.WorldSettings.AutoIdentify.CurrentValue;
        }

        public void ClearKnownItems(IMap map)
        {
            _knownItemNames.Clear();
            map.ItemPlaceholders.ClearPlayerAssignedNames();
            map.Events.Record(new MemoryLost(Entity.Ref, Label, MemoryKind.ItemNames, this.WholeInventoryLookIn(map), map.PlayerUnderfoot()));
        }

        #endregion

        public void ListenToAlert(Location location)
        {
            _statusManager.RemoveConditionType(typeof(Slept));
            _behavior.KnowLocationOf(location);
        }

        public bool IsVisible(Vector2Int position)
        {
            return VisionRange.IsVisible(position);
        }

        public void OnAttackedBy(IActorOfEffect actor, float impact)
        {
            _statusManager.WasAttacked();
            if (!IsVisible(actor.Entity.CurrentPosition))
                return;

            var direction =
                DirectionMethods.NearestDirectionFromVector(actor.Entity.CurrentPosition - Entity.CurrentPosition);
            if (direction.HasValue)
            {
                Turn(direction.Value);
            }

            _affiliationManager.OnCharacterAttacked(actor.Affiliation, Affiliation, impact);
        }

        public void OnHealedBy(IActorOfEffect actor, float impact)
        {
            if (!IsVisible(actor.Entity.CurrentPosition))
                return;

            _affiliationManager.OnCharacterHealed(actor.Affiliation, Affiliation, impact);
        }


        public void ClearAffiliation(IMap map)
        {
            _affiliationManager.Clear();
            map.Events.Record(new MemoryLost(Entity.Ref, Label, MemoryKind.Characters, null, null));
        }

        public bool CanPickUpItem()
        {
            return _inventory.HasEmptySpace();
        }

        public void AddEvent(IPlayerEvent ev)
        {
            _events.Add(ev);
        }

        public async UniTask<int?> SelectItem(string text, params int[] disabledItems)
        {
            var disabledItemIndexes = disabledItems.Select(x => new ItemFocus(x)).ToList();
            disabledItemIndexes.Add(ItemFocus.GroundItem);
            var focus = await _behavior.SelectItem(text, disabledItemIndexes.ToArray());
            if (focus.IsInInventory)
                return focus.Index;
            else if (focus.IsOnEmpty)
                return null;
            else
                throw new Exception("Unexpected item focus");
        }

        public async UniTask<int?> SelectItemWithCanSelect(string text, Func<IItem, bool> canSelect)
        {
            var disabledItemFocuses = new List<ItemFocus>();
            foreach (var (item, index) in Inventory.AllItemsWithIndex)
            {
                if (!canSelect(item))
                {
                    disabledItemFocuses.Add(new ItemFocus(index));
                }
            }
            disabledItemFocuses.Add(ItemFocus.GroundItem);

            var focus = await _behavior.SelectItem(text, disabledItemFocuses.ToArray());
            if (focus.IsInInventory)
                return focus.Index;
            else if (focus.IsOnEmpty)
                return null;
            else
                throw new Exception("Unexpected item focus");
        }

        public async UniTask<int?> SelectItemWithCanSelectPreview(
            string text,
            Func<IItem, bool> canSelect,
            Func<IItem, ItemSelectPreview?> buildPreview,
            ItemSelectPreview? defaultPreview,
            string previewTitle)
        {
            var disabledItemFocuses = new List<ItemFocus>();
            var previews = new List<ItemSelectPreview>();
            foreach (var (item, index) in Inventory.AllItemsWithIndex)
            {
                var preview = buildPreview(item);
                if (preview != null)
                {
                    previews.Add(preview with { Focus = new ItemFocus(index) });
                }
                if (!canSelect(item))
                {
                    disabledItemFocuses.Add(new ItemFocus(index));
                }
            }
            disabledItemFocuses.Add(ItemFocus.GroundItem);

            var focus = await _behavior.SelectItemWithPreview(
                text,
                disabledItemFocuses.ToArray(),
                previews.ToArray(),
                defaultPreview,
                previewTitle);
            if (focus.IsInInventory)
                return focus.Index;
            else if (focus.IsOnEmpty)
                return null;
            else
                throw new Exception("Unexpected item focus");
        }

        public async UniTask<ItemFocus> SelectItemContainsGroundItem(string text, params ItemFocus[] disabledItems)
        {
            return await _behavior.SelectItem(text, disabledItems);
        }

        public async UniTask<ItemFocus> SelectItemWithCanSelectContainsGroundItem(string text, IPlayer player, IMap map, Func<IItem, bool> canSelect)
        {
            var disabledItemIndexes = new List<ItemFocus>();
            foreach (var (item, index) in Inventory.AllItemsWithIndex)
            {
                if (!canSelect(item))
                {
                    disabledItemIndexes.Add(new ItemFocus(index));
                }
            }

            var groundItem = map.Items.At(player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
            if (groundItem == null || !canSelect(groundItem))
            {
                disabledItemIndexes.Add(ItemFocus.GroundItem);
            }

            return await SelectItemContainsGroundItem(text, disabledItemIndexes.ToArray());
        }

        public async UniTask UpdateTurn()
        {
            var visibleCharacters = _map.GetVisibleCharacters(this);
            await _statusManager.UpdateTurn(visibleCharacters.Any());
            _affiliationManager.UpdateTurn(visibleCharacters.Select(x => x.Affiliation));
            if (_statusManager.IsFlagStat(FlagStatType.RandomTeleport) && RandUtils.IsLessThanProbability(CommonSenseParameters.RandomTeleportProbability))
            {
                var memento = SkillWithCost.Build(
                    new SkillData(
                        position: new AtFeet(),
                        area: new SelfArea(),
                        effects: new List<IEffect> { new TeleportEffect() },
                        repeats: 1,
                        probabilityOfSuccess: 1,
                        cost: 0,
                        rushDistance: 0,
                        backStepDistance: 0,
                        chargeTurn: 0,
                        coolTime: 0,
                        log: "はテレポートした"
                    ));
                var skill = new SkillWithCost(memento);
                await UseSkill(skill, CurrentDirection, _map);
            }
            if (_statusManager.IsFlagStat(FlagStatType.RandomExplosion) && RandUtils.IsLessThanProbability(CommonSenseParameters.RandomExplosionProbability))
            {
                var memento = SkillWithCost.Build(
                    new SkillData(
                        position: new AtFeet(),
                        area: new CircleArea(2, true, false),
                        effects: new List<IEffect> { new PercentageDamageEffect(0.25f), new BreakEffect(false, true, true, true, true, true) },
                        repeats: 1,
                        probabilityOfSuccess: 1,
                        cost: 0,
                        rushDistance: 0,
                        backStepDistance: 0,
                        chargeTurn: 0,
                        coolTime: 0,
                        log: "は爆発した"
                    ));
                var skill = new SkillWithCost(memento);
                await UseSkill(skill, CurrentDirection, _map);
            }
            _inventory.UpdateTurn(_map);
        }

        public void UpdateCharacterTurn()
        {
            _skills.ForEach(x => x.Skill.CoolDown());
        }
    }
}