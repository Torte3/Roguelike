using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Model.Entity
{
    public interface IEntity : IDisposable
    {
        public EntityBase Entity { get; }
        public bool IsGrounded { get; }
        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map);
        public WorldEvent Appeared(IMap map);
        public EntityLabel LabelIn(IMap map);
        public bool CanBeBrokenBy(BreakTargets targets) => false;
        public void Break(IMap map)
        {
            map.Events.Record(new EntityBroken(Entity.IsVisible, LabelIn(map)));
            Entity.Destroy();
        }
    }
}
