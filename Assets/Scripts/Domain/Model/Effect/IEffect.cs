using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Entity;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IEffect : IHasInfo
    {
        public Impact Impact { get; }
        public Color Color { get; }

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map);
        public UniTask Apply(IActorOfEffect actor, IEntity target, Vector2Int position, IMap map);
        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map);

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target);
        public float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions);
        public float EvaluatePrice();
    }
}