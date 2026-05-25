using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Effect;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Trap : ISerializable<TrapMemento>, IEntityEventEntity
    {
        public readonly string Name;
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;
        private readonly SpawnActorlessEffectSkill _skill;
        private readonly float _probabilityOfBreaking;

        public Trap(TrapMemento memento)
        {
            Name = memento.Name;
            Entity = new EntityBase(memento.Entity);
            _skill = new SpawnActorlessEffectSkill(memento.Skill);
            _probabilityOfBreaking = memento.ProbabilityOfBreaking;
            Event = new EntityEvent(
                entity => entity.IsGrounded ||
                          (entity is ICharacter character &&
                           character.Status.IsFlagStat(FlagStatType.IsAffectedByTrap)),
                async (_, gameManager, map) =>
                {
                    gameManager.PlaySE(SE.TrapStep);
                    await Execute(map);
                }
            );
        }

        public IEntityEvent Event { get; init; }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public TrapMemento Serialize()
        {
            return new TrapMemento(Name, Entity.Serialize(), _skill.Serialize(), _probabilityOfBreaking);
        }

        public static TrapMemento Build(TrapData trap, Vector2Int position)
        {
            return new TrapMemento(trap.name, EntityBase.Build(position, EntityLayer.Floor),
                SpawnActorlessEffectSkill.Build(trap.Skill), trap.ProbabilityOfBreaking);
        }

        public async UniTask Execute(IMap map)
        {
            GameLog.Add(Entity.IsVisible, $"<color=red>{Name}</color>が起動した");
            await _skill.Use(Name, Entity.CurrentPosition, map, Entity.Id);
            if (Random.value < _probabilityOfBreaking)
            {
                GameLog.Add(Entity.IsVisible, $"<color=red>{Name}</color>は壊れた");
                Entity.Destroy("は壊れた");
            }
        }
    }
}