using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using UnityEngine;
using Utilities;

namespace Domain.Service.Events
{
    public class Teleporter : ISerializable<EntityMemento>, IEntityEventEntity, IIconEntity
    {
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;

        public Teleporter(EntityMemento memento)
        {
            Entity = new EntityBase(memento);
            Event = new EntityEvent(
                entity => entity.IsGrounded,
                (entity, _, map) =>
                {
                    entity.Entity.Teleport(map.GetAllBlankAndStandablePositionsOn(EntityLayer.Middle).GetAtRandom().Position);
                    return UniTask.CompletedTask;
                }
            );
        }

        public Sprite Icon => ObjectLoader.LoadMapChip("(Base)BaseChip_pipo_71");

        public IEntityEvent Event { get; init; }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public EntityMemento Serialize()
        {
            return Entity.Serialize();
        }

        public static EntityMemento Build(Vector2Int position)
        {
            return EntityBase.Build(position, EntityLayer.Floor);
        }
    }
}