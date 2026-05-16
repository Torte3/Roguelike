using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Effect;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Stats;

namespace Domain.Service.Events
{
    public class Statue : ISerializable<StatueMemento>, IScheduledEventEntity, IIconEntity
    {
        public readonly string Name;
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;
        private readonly SpawnActorlessEffectSkill _skill;
        public StatueType Type;
        private int _attackToBreak;
        private readonly Subject<Unit> _onAttacked = new();
        public Observable<Unit> OnAttacked => _onAttacked;

        public Statue(StatueMemento memento)
        {
            Name = memento.Name;
            Entity = new EntityBase(memento.Entity);
            _skill = new SpawnActorlessEffectSkill(memento.Skill);
            Type = memento.Type;
            _attackToBreak = memento.AttackToBreak;
            Event = new ScheduledEvent(
                memento.Cycle,
                async (gameManager, map) => { await Execute(map); }
            );
        }

        public IScheduledEvent Event { get; init; }

        public Sprite Icon => Type switch
        {
            StatueType.Beneficial => ObjectLoader.LoadMapChip("(Base)BaseChip_pipo_923"),
            StatueType.Harmful => ObjectLoader.LoadMapChip("(Base)BaseChip_pipo_924"),
            StatueType.Neutral => ObjectLoader.LoadMapChip("(Base)BaseChip_pipo_908"),
            _ => throw new NotImplementedException()
        };

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
                Name,
                Entity.Serialize(),
                _skill.Serialize(),
                Type,
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
            GameLog.Add(Entity.IsVisible, $"<color=red>{Name}</color>が起動した");
            await _skill.Use(Name, Entity.CurrentPosition, map);
        }

        public void Attacked()
        {
            _attackToBreak -= 1;
            if (_attackToBreak <= 0)
            {
                GameLog.Add(Entity.IsVisible, $"<color=red>{Name}</color>は壊れた");
                Entity.Destroy($"は壊された");
            }
            _onAttacked.OnNext(Unit.Default);
        }
    }
}