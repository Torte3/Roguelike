#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character.Type;
using Domain.Model.Condition;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
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
        public bool IsDead { get; }
        public bool IsLeader { get; }
        public bool IsBoss { get; }
        public CharacterState State { get; }
        public void SetWaitState();
        public ReadOnlyReactiveProperty<bool> HasEvent { get; }
        public ReadOnlyReactiveProperty<bool> AutoIdentify { get; }
        public ReadOnlyReactiveProperty<bool> CurseAutoIdentify { get; }
        public Health Health { get; }
        public bool CanMove(Vector2Int position, Direction8 direction, bool isFlying, bool canThroughWalls,
            IPassableChecker map);

        public void RememberTerrainBefore(IReadOnlyList<(Vector2Int Position, TileData Tile)> previousTiles);
        public UniTask UseItemOnDeath();
        public UniTask UseLastSkill();
        public void Die(DamageSource source);
        public Observable<DeathRecord> OnPerished { get; }
        public void ApplyKillHealToAttacker(ICharacter? attacker);
        public void OnAttackedBy(IActorOfEffect actor, float impact);
        public void OnHealedBy(IActorOfEffect actor, float impact);
        public UniTask DoNextAction(IGameManager gameManager, IMap map, IInput input);
        public void ResetChargeAction();
        public bool CanPickUpItem();
        public void AddEvent(IPlayerEvent ev);
        public UniTask UpdateTurn();
        public void UpdateCharacterTurn();
    }
}