using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using Domain.Service.Effect;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Trap : ISerializable<TrapMemento>, IEntityEventEntity
    {
        private readonly string _name;
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;
        private readonly SpawnActorlessEffectSkill _skill;
        private readonly float _probabilityOfBreaking;

        public Trap(TrapMemento memento)
        {
            _name = memento.Name;
            Entity = new EntityBase(memento.Entity);
            _skill = new SpawnActorlessEffectSkill(memento.Skill);
            _probabilityOfBreaking = memento.ProbabilityOfBreaking;
            Event = new EntityEvent(
                entity => entity.IsGrounded ||
                          (entity is ICharacter character &&
                           character.Status.IsFlagStat(FlagStatType.IsAffectedByTrap)),
                async (_, _, map) => { await Execute(map); }
            );
        }

        public IEntityEvent Event { get; init; }

        public bool CanBeBrokenBy(BreakTargets targets) => targets.HasFlag(BreakTargets.Trap);

        public EntityLabel LabelIn(IMap map)
        {
            return new NamedEntityLabel(_name);
        }

        public WorldEvent Appeared(IMap map)
        {
            return Entity.Appeared(EntityKind.Trap, null);
        }

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
            return new TrapMemento(_name, Entity.Serialize(), _skill.Serialize(), _probabilityOfBreaking);
        }

        public static TrapMemento Build(TrapData trap, Vector2Int position)
        {
            return new TrapMemento(trap.name, EntityBase.Build(position, EntityLayer.Floor),
                SpawnActorlessEffectSkill.Build(trap.Skill), trap.ProbabilityOfBreaking);
        }

        private async UniTask Execute(IMap map)
        {
            map.Events.Record(new SkillUsed(Entity.Ref, new DeviceSkillSource(_name, DeviceKind.Trap)));
            await _skill.Use(Entity.CurrentPosition, map, Entity.Id);
            if (Random.value < _probabilityOfBreaking)
            {
                map.Events.Record(new DeviceBroken(Entity.IsVisible, _name));
                Entity.Destroy();
            }
        }
    }
}