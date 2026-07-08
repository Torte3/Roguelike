using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using Domain.Service.Effect;
using UnityEngine;
using Utilities;
using Utilities.Stats;

namespace Domain.Service.Events
{
    public class Statue : ISerializable<StatueMemento>, IScheduledEventEntity
    {
        private readonly string _name;
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;
        private readonly SpawnActorlessEffectSkill _skill;
        private readonly StatueType _type;
        private int _attackToBreak;

        public Statue(StatueMemento memento)
        {
            _name = memento.Name;
            Entity = new EntityBase(memento.Entity);
            _skill = new SpawnActorlessEffectSkill(memento.Skill);
            _type = memento.Type;
            _attackToBreak = memento.AttackToBreak;
            Event = new ScheduledEvent(
                memento.Cycle,
                async (gameManager, map) => { await Execute(map); }
            );
        }

        public IScheduledEvent Event { get; init; }

        private Sprite Icon => _type switch
        {
            StatueType.Beneficial => ObjectLoader.LoadMapChip("(Base)BaseChip_pipo_923"),
            StatueType.Harmful => ObjectLoader.LoadMapChip("(Base)BaseChip_pipo_924"),
            StatueType.Neutral => ObjectLoader.LoadMapChip("(Base)BaseChip_pipo_908"),
            _ => throw new NotImplementedException()
        };

        public bool CanBeBrokenBy(BreakTargets targets) => targets.HasFlag(BreakTargets.Statue);

        public EntityLabel LabelIn(IMap map)
        {
            return new NamedEntityLabel(_name);
        }

        public WorldEvent Appeared(IMap map)
        {
            return Entity.Appeared(EntityKind.Statue, Icon);
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public StatueMemento Serialize()
        {
            return new StatueMemento(
                _name,
                Entity.Serialize(),
                _skill.Serialize(),
                _type,
                Event.WaitTurnData,
                _attackToBreak);
        }

        public static StatueMemento Build(StatueData statue, Vector2Int position)
        {
            return new StatueMemento(
                name: statue.name,
                entity: EntityBase.Build(position, EntityLayer.Middle),
                skill: SpawnActorlessEffectSkill.Build(statue.Skill),
                type: statue.Type,
                cycle: new ResourceData(
                    new StatData(statue.Cycle, minValue: 0f),
                    statue.Cycle),
                attackToBreak: statue.AttackToBreak);
        }

        private async UniTask Execute(IMap map)
        {
            map.Events.Record(new SkillUsed(Entity.Ref, new DeviceSkillSource(_name, DeviceKind.Statue)));
            await _skill.Use(Entity.CurrentPosition, map);
        }

        public void Attacked(IMap map)
        {
            map.Events.Record(new StatueStruck(Entity.Ref));
            _attackToBreak -= 1;
            if (_attackToBreak <= 0)
            {
                map.Events.Record(new DeviceBroken(Entity.IsVisible, _name));
                Entity.Destroy();
            }
        }
    }
}