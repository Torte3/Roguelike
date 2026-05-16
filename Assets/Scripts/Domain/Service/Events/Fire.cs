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
    public class Fire : ISerializable<EntityMemento>, IEntity
    {
        public EntityBase Entity { get; init; }
        public bool IsGrounded => false;

        public Fire(EntityMemento memento)
        {
            Entity = new EntityBase(memento);
        }

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
            return EntityBase.Build(position, EntityLayer.Top);
        }
    }
}