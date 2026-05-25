using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Entity;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Model.Effect
{
    public abstract class ActorlessEntityTargetEffect : IActorlessEffect
    {
        public abstract Impact Impact { get; }
        public abstract Color Color { get; }

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map)
        {
            return Apply(target, position, map);
        }

        public virtual UniTask Apply(ITargetOfEffect target, Vector2Int position, IMap map)
        {
            return Apply((IEntity)target, position, map);
        }

        public UniTask Apply(IActorOfEffect actor, IEntity target, Vector2Int position, IMap map)
        {
            return Apply(target, position, map);
        }

        public virtual UniTask Apply(IEntity target, Vector2Int position, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            return Apply(positions, map);
        }

        public UniTask Apply(IEnumerable<Vector2Int> positions, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public abstract float Evaluate(IActorOfEffect actor, ITargetOfEffect target);

        public float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions)
        {
            return 0;
        }

        public abstract float EvaluatePrice();

        public abstract string Info();
    }
}