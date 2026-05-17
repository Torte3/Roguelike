#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Character.Message;
using Domain.Model.Character.Type;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Model.Character
{
    public interface ICharacter : IDisposable, ISerializable<CharacterMemento>, IEntity, IHasBehavior,
        IHasCondition, IPlayerEventEntity
    {
        public ICharacterType CharacterType { get; init; }
        public string Name { get; }
        public bool IsPlayer { get; }
        public bool IsLeader { get; }
        public bool IsBoss { get; }
        public CharacterState State { get; }
        public void SetWaitState();
        public bool IsDead { get; }
        public ReadOnlyReactiveProperty<Direction8> Direction { get; }
        public ReadOnlyReactiveProperty<bool> HasEvent { get; }
        public ReadOnlyReactiveProperty<bool> AutoIdentify { get; }
        public ReadOnlyReactiveProperty<bool> CurseAutoIdentify { get; }
        public Observable<Unit> OnAttacked { get; }
        public Observable<Unit> OnDead { get; }
        public Observable<OnStartItemSelectMessage> OnStartItemSelect { get; }
        public Observable<Unit> OnSelectedItemSelect { get; }
        public IObservableCollection<string> KnownItemNames { get; }
        public Observable<OnChargeActionUpdatedMessage> OnChargeActionUpdated { get; }
        public Observable<string> OnItemUsed { get; }
        public bool CanMove(Vector2Int position, Direction8 direction, bool isFlying, bool canThroughWalls,
            IPassableChecker map);

        public bool CanMoveIgnoreEntity(Vector2Int position, Direction8 direction, IPassableChecker map);
        public UniTask ForceMove(Direction8 direction, IInput input);
        public UniTask UseItemOnDeath();
        public UniTask UseLastSkill();
        public void Die(string causeOfDeathLog);
        public void OnAttackedBy(IActorOfEffect actor, float impact);
        public void OnHealedBy(IActorOfEffect actor, float impact);
        public UniTask DoNextAction(IGameManager gameManager, IMap map, IInput input);
        public void ResetChargeAction();
        public bool CanPickUpItem();
        public void AddEvent(IPlayerEvent ev);
        public UniTask UpdateTurn();
        public void UpdateCharacterTurn();

        public bool IsVisible(Vector2Int position)
        {
            return VisionRange.IsVisible(position);
        }
    }
}