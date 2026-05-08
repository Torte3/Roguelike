#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Action;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;
using Random = UnityEngine.Random;

namespace Domain.Service.Characters.Behavior
{
    public sealed class EnemyBehavior : ICharacterBehavior
    {
        public Observable<OnStartItemSelectMessage> OnStartItemSelect { get; init; } = new Subject<OnStartItemSelectMessage>();
        public Observable<Unit> OnSelectedItemSelect { get; init; } = new Subject<Unit>();

        private BehaviorResult _previousResult;
        private readonly Option<Location> _homeLocation = Option.None<Location>();

        private readonly float behavioralRandomness = 0.01f;

        private readonly MoveTypeWhenUndiscoveringTarget _wander;

        private MoveTypeWhenDiscoveringTarget _discoveringLeader => BehaviorData.ChaseLeader ? MoveTypeWhenDiscoveringTarget.Chase : MoveTypeWhenDiscoveringTarget.Wander;
        private readonly MoveTypeWhenDiscoveringTarget _returningHome = MoveTypeWhenDiscoveringTarget.Chase;
        private readonly MoveTypeWhenDiscoveringTarget _default;
        private readonly bool _prioritizeMovement;

        private readonly float _distanceTopBound = float.PositiveInfinity;
        private readonly MoveTypeWhenDiscoveringTarget? _greaterThanTopBound;
        private readonly bool _prioritizeMovementWhenDistanceGreaterThanTopBound;

        private readonly float _distanceBottomBound = float.NegativeInfinity;
        private readonly MoveTypeWhenDiscoveringTarget? _lessThanBottomBound;
        private readonly bool _prioritizeMovementWhenDistanceLessThanBottomBound;

        public BehaviorData BehaviorData { get; init; }

        public EnemyBehavior(BehaviorMemento data, Id<IMap> mapId)
        {
            BehaviorData = data.Behavior;
            if (data.HomeLocation.HasValue && data.HomeLocation.Value!.MapId == mapId)
            {
                _homeLocation = data.HomeLocation;
            }

            _previousResult = new BehaviorResult(
                data.PreviousState.Value,
                data.PreviousTargetLocation
            );

            if (BehaviorData.wanderAround)
            {
                _wander = MoveTypeWhenUndiscoveringTarget.Wander;
            }
            else
            {
                _wander = MoveTypeWhenUndiscoveringTarget.NoMove;
            }

            _default = BehaviorData.Default;
            _prioritizeMovement = BehaviorData.PrioritizeMovement;
            if (BehaviorData.UseTopBound)
            {
                _distanceTopBound = BehaviorData.distanceTopBound;
                _greaterThanTopBound = BehaviorData.greaterThanTopBound;
                _prioritizeMovementWhenDistanceGreaterThanTopBound =
                    BehaviorData.PrioritizeMovementWhenDistanceGreaterThanTopBound;
            }

            if (BehaviorData.UseBottomBound)
            {
                _distanceBottomBound = BehaviorData.distanceBottomBound;
                _lessThanBottomBound = BehaviorData.lessThanBottomBound;
                _prioritizeMovementWhenDistanceLessThanBottomBound =
                    BehaviorData.PrioritizeMovementWhenDistanceLessThanBottomBound;
            }
        }

        public BehaviorMemento Serialize()
        {
            return new BehaviorMemento(
                BehaviorData,
                _homeLocation,
                Option.Some(_previousResult.State),
                _previousResult.TargetLocation
            );
        }

        public static BehaviorMemento Build(BehaviorData behavior, Option<Location> homeLocation)
        {
            var memento = new BehaviorMemento(
                behavior,
                homeLocation,
                Option<BehaviorState>.None,
                Option<Location>.None
            );
            var json = JsonUtility.ToJson(memento);
            return JsonUtility.FromJson<BehaviorMemento>(json);
        }

        public async UniTask<IAction> GenerateNextAction(IHasBehavior character, IGameManager gameManager, IMap map,
            IInput input)
        {
            var result = GenerateNextBehaviorResult(character, map);
            Log.Debug($"[Think]Result: {result.State} {result.TargetLocation}");

            if (result.TargetLocation.HasValue && result.TargetLocation.Value!.MapId == map.Id)
            {
                var relativeVector = result.TargetLocation.Value.Position - character.Entity.CurrentPosition;
                if (VectorExtension.ChebyshevDistance(relativeVector) <= 1)
                {
                    var direction = DirectionMethods.NearestDirectionFromVector(relativeVector);
                    if (direction.HasValue)
                        character.Turn(direction.Value);
                }
            }

            var actions = new List<(IAction action, float evaluate)>();
            if (!result.IsDiscoveringEnemy())
            {
                actions.AddRange(GenerateValidMoves(character, result, map));
            }
            else if (PrioritizeMovement(character, result.TargetLocation, map.Id))
            {
                Log.Debug("[Think]Prioritize Movement.");
                actions.AddRange(GenerateValidMoves(character, result, map));
                if (!actions.Any())
                {
                    actions.AddRange(GenerateValidUseSkills(character, map));
                    actions.AddRange(GenerateValidUseItems(character, map));
                    actions.AddRange(GenerateValidThrowItems(character, map));
                }
            }
            else
            {
                Log.Debug("[Think]Not Prioritize Movement.");
                actions.AddRange(GenerateValidUseSkills(character, map));
                actions.AddRange(GenerateValidUseItems(character, map));
                actions.AddRange(GenerateValidThrowItems(character, map));
                if (!actions.Any())
                {
                    actions.AddRange(GenerateValidMoves(character, result, map));
                }
            }

            foreach (var actionTemp in actions)
            {
                Log.Debug($"[Think]{actionTemp.action.Info()} {actionTemp.evaluate}");
            }

            var action = await UniTask.FromResult(actions.MaxByOrDefault(
                action => action.evaluate + Random.Range(0, behavioralRandomness),
                (action: new DoNothing(), evaluate: 0)));

            _previousResult = result;
            return action.action;
        }

        private BehaviorResult GenerateNextBehaviorResult(IHasBehavior character, IMap map)
        {
            var visibleEnemies = map.Characters.ByAffiliation(character, AffiliationType.Enemy)
                .IsVisible(character.Entity.CurrentPosition);
            var visibleLeaders = map.Characters.Where(c => c.IsLeader).ByAffiliation(character, AffiliationType.Ally)
                .IsVisible(character.Entity.CurrentPosition);
            if (character.IsAlly(map.Player.Character))
            {
                visibleLeaders = visibleLeaders.Append(map.Player.Character);
            }

            var targetedEnemy = visibleEnemies.MinByOrDefault(
                enemy => VectorExtension.ChebyshevDistance(character.Entity.CurrentPosition,
                    enemy.Entity.CurrentPosition), null);
            var targetedLeader =
                visibleLeaders.MaxByOrDefault(leader => character.Affiliation.GetAffection(leader.Affiliation), null);

            // 優先順位に基づいてターゲットを選択
            var primaryTarget = BehaviorData.PrioritizeEnemiesOverLeaders ? targetedEnemy : targetedLeader;
            var secondaryTarget = BehaviorData.PrioritizeEnemiesOverLeaders ? targetedLeader : targetedEnemy;
            var primaryState = BehaviorData.PrioritizeEnemiesOverLeaders ?
                BehaviorState.DiscoveringEnemy : BehaviorState.DiscoveringLeader;
            var secondaryState = BehaviorData.PrioritizeEnemiesOverLeaders ?
                BehaviorState.DiscoveringLeader : BehaviorState.DiscoveringEnemy;

            if (primaryTarget != null)
            {
                return new BehaviorResult(primaryState, Option.Some(new Location(map.Id, primaryTarget.Entity.CurrentPosition)));
            }

            if (secondaryTarget != null)
            {
                return new BehaviorResult(secondaryState, Option.Some(new Location(map.Id, secondaryTarget.Entity.CurrentPosition)));
            }

            if (_previousResult.State == BehaviorState.ApproachingToObserve)
            {
                if (CanReachButNotAtTarget(character, _previousResult.TargetLocation.Value, map)
                    && !character.VisionRange.IsVisible(_previousResult.TargetLocation.Value.Position))
                {
                    return _previousResult;
                }
            }
            else if (_homeLocation.HasValue)
            {
                return new BehaviorResult(BehaviorState.ReturningHome, _homeLocation);
            }

            if (_previousResult.State == BehaviorState.MovingToLastKnownEnemyPosition
                && CanReachButNotAtTarget(character, _previousResult.TargetLocation.Value, map))
            {
                return _previousResult;
            }

            if (_previousResult.State == BehaviorState.DiscoveringEnemy
                && IsChasingEnemy()
                && CanReachButNotAtTarget(character, _previousResult.TargetLocation.Value, map))
            {
                return new BehaviorResult(BehaviorState.MovingToLastKnownEnemyPosition, _previousResult.TargetLocation);
            }

            if (_previousResult.State == BehaviorState.MovingToLastKnownLeaderPosition
                && CanReachButNotAtTarget(character, _previousResult.TargetLocation.Value, map))
            {
                return _previousResult;
            }

            if (_previousResult.State == BehaviorState.DiscoveringLeader
                && CanReachButNotAtTarget(character, _previousResult.TargetLocation.Value, map))
            {
                return new BehaviorResult(BehaviorState.MovingToLastKnownLeaderPosition,
                    _previousResult.TargetLocation);
            }

            return new BehaviorResult(BehaviorState.Wandering, Option.None<Location>());
        }

        public bool CanReachButNotAtTarget(IHasBehavior character, Location targetLocation, IMap map)
        {
            if (targetLocation.MapId != map.Id)
            {
                return false;
            }
            return character.Entity.CurrentPosition != targetLocation.Position
                   && map.IsReachable(character.Entity.CurrentPosition, targetLocation.Position, character);
        }

        public bool IsChasingEnemy()
        {
            return _default is Chase && (_greaterThanTopBound == null || _greaterThanTopBound is Chase) &&
                   (_lessThanBottomBound == null || _lessThanBottomBound is Chase);
        }

        private float GetDistance(IHasBehavior character, Vector2Int targetPosition)
        {
            var distance = VectorExtension.ChebyshevDistance(character.Entity.CurrentPosition, targetPosition);
            return distance;
        }

        public MoveTypeWhenDiscoveringTarget GetMoveTypeWhenDiscoveringTarget(IHasBehavior character,
            Vector2Int targetPosition, BehaviorState state)
        {
            switch (state)
            {
                case BehaviorState.DiscoveringLeader:
                    return _discoveringLeader;
                case BehaviorState.ReturningHome:
                    return _returningHome;
                case BehaviorState.ApproachingToObserve:
                    return MoveTypeWhenDiscoveringTarget.Chase;
                case BehaviorState.DiscoveringEnemy:
                    var distance = GetDistance(character, targetPosition);
                    if (_greaterThanTopBound != null && distance > _distanceTopBound)
                    {
                        Log.Debug($"[Think]Distance is greater than top bound {_distanceTopBound}.");
                        return _greaterThanTopBound.Value;
                    }

                    if (_lessThanBottomBound != null && distance < _distanceBottomBound)
                    {
                        Log.Debug($"[Think]Distance is less than bottom bound {_distanceBottomBound}.");
                        return _lessThanBottomBound.Value;
                    }

                    Log.Debug("[Think]Behavior is Default.");
                    return _default;
                case BehaviorState.MovingToLastKnownEnemyPosition:
                case BehaviorState.MovingToLastKnownLeaderPosition:
                    return MoveTypeWhenDiscoveringTarget.Chase;
                case BehaviorState.Wandering:
                    throw new Exception("Wandering is not a valid state for discovered target behavior.");
                default:
                    throw new Exception($"Invalid state: {state}");
            }
        }

        public bool PrioritizeMovement(IHasBehavior character, Option<Location> targetLocation, Id<IMap> mapId)
        {
            if (!targetLocation.HasValue || targetLocation.Value!.MapId != mapId)
                return _prioritizeMovement;
            var distance = GetDistance(character, targetLocation.Value.Position);
            if (distance > _distanceTopBound)
                return _prioritizeMovementWhenDistanceGreaterThanTopBound;
            if (distance < _distanceBottomBound)
                return _prioritizeMovementWhenDistanceLessThanBottomBound;
            return _prioritizeMovement;
        }

        private IEnumerable<(IAction action, float evaluate)> GenerateValidMoves(IHasBehavior character, BehaviorResult result, IMap map)
        {
            return GenerateDoableMoves(character, result, map).Select(move => (action: move, evaluate: move.Evaluate(character, map)));
        }

        private IEnumerable<IAction> GenerateDoableMoves(IHasBehavior character, BehaviorResult result,
            IMap map)
        {
            if (result.TargetLocation.HasValue && result.TargetLocation.Value!.MapId == map.Id)
            {
                var moveType = GetMoveTypeWhenDiscoveringTarget(character, result.TargetLocation.Value.Position, result.State);
                return MoveGenerater.GenerateDoableMovesWhenDiscoveringTarget(moveType, character, result.TargetLocation.Value.Position, map);
            }

            return MoveGenerater.GenerateDoableMovesWhenUndiscoveringTarget(_wander, character, map);
        }

        private IEnumerable<(IAction action, float evaluate)> GenerateValidUseSkills(IHasBehavior character, IMap map)
        {
            // スキルを優先度でグループ化（大きい順）
            var skillGroups = character.Skills
                .GroupBy(skill => skill.Priority)
                .OrderByDescending(group => group.Key)
                .Select(group => group.Select(skill => skill.Skill));

            foreach (var skillGroup in skillGroups)
            {
                var groupActions = new List<UseSkill>();

                foreach (var skill in skillGroup)
                {
                    if (skill.Skill.IsDirectional)
                    {
                        groupActions.AddRange(
                            DirectionMethods.AllDirections.Select(direction => new UseSkill(skill, direction)));
                    }
                    else
                    {
                        groupActions.Add(new UseSkill(skill, character.CurrentDirection));
                    }
                }

                var validActions = groupActions
                    .Where(action => action.Doable(character, map))
                    .Select(action => (skill: (IAction)action, evaluate: action.Evaluate(character, map)))
                    .Where(action => action.evaluate > 0);

                if (validActions.Any())
                {
                    return validActions;
                }
            }

            return Enumerable.Empty<(IAction action, float evaluate)>();
        }

        private IEnumerable<(IAction action, float evaluate)> GenerateValidUseItems(IHasBehavior character, IMap map)
        {
            if (!character.CanUseItem)
            {
                return Enumerable.Empty<(IAction action, float evaluate)>();
            }

            var actions = new List<UseItem>();
            foreach (var item in character.Inventory.AllItems)
            {
                if (!item.CanActivateWhenUsed)
                    continue;
                if (!character.CanReadItem && item.RequiresLiteracy)
                    continue;

                if (item.SkillOnUse.Expect("SkillOnUse is null").Skill.IsDirectional)
                {
                    actions.AddRange(DirectionMethods.AllDirections.Select(direction => new UseItem(item, direction)));
                }
                else
                {
                    actions.Add(new UseItem(item, character.CurrentDirection));
                }
            }

            return actions
                .Where(action => action.Doable(character, map))
                .Select(action => (item: (IAction)action, evaluate: action.Evaluate(character, map)))
                .Where(action => action.evaluate > 0);
        }

        private IEnumerable<(IAction action, float evaluate)> GenerateValidThrowItems(IHasBehavior character, IMap map)
        {
            if (!character.CanUseItem)
            {
                return Enumerable.Empty<(IAction action, float evaluate)>();
            }

            var actions = new List<ThrowItem>();
            foreach (var item in character.Inventory.AllItems)
            {
                if (!item.CanActivateWhenThrown)
                    continue;
                if (!character.CanReadItem && item.RequiresLiteracy)
                    continue;

                actions.AddRange(DirectionMethods.AllDirections.Select(direction => new ThrowItem(item, direction)));
            }

            return actions
                .Where(action => action.Doable(character, map))
                .Select(action => (item: (IAction)action, evaluate: action.Evaluate(character, map)))
                .Where(action => action.evaluate > 0);
        }

        public void KnowLocationOf(Location location)
        {
            _previousResult = new BehaviorResult(
                BehaviorState.ApproachingToObserve,
                Option.Some(location)
            );
        }

        public UniTask<ItemFocus> SelectItem(string text, ItemFocus[] disabledItems)
        {
            return UniTask.FromResult(ItemFocus.Empty);
        }

        public UniTask<ItemFocus> SelectItemWithPreview(string text, ItemFocus[] disabledItems, ItemSelectPreview[] previews, ItemSelectPreview? defaultPreview, string previewTitle)
        {
            return UniTask.FromResult(ItemFocus.Empty);
        }
    }
}