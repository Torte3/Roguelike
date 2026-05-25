using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class RandomEffect : IEffect
    {
        [SerializeReference] public List<IEffect> Effects;
        private int _index = -1;
        private bool _notRandomized = true;

        public Color Color => _index == -1 ? Colors.White : Effects[_index].Color;

        public Impact Impact => Impact.Neutral;

        public UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map)
        {
            if (_notRandomized)
            {
                _index = Enumerable.Range(0, Effects.Count).GetAtRandom();
                _notRandomized = false;
            }

            return Effects[_index].Apply(actor, target, position, map);
        }

        public UniTask Apply(IActorOfEffect actor, IEntity target, Vector2Int position, IMap map)
        {
            if (_notRandomized)
            {
                _index = Enumerable.Range(0, Effects.Count).GetAtRandom();
                _notRandomized = false;
            }

            return Effects[_index].Apply(actor, target, position, map);
        }

        public UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            if (_notRandomized)
            {
                _index = Enumerable.Range(0, Effects.Count).GetAtRandom();
                _notRandomized = false;
            }

            var effect = Effects[_index];
            _notRandomized = true;
            return effect.Apply(actor, positions, map);
        }

        private int ImpactValue(Impact impact)
        {
            return impact switch
            {
                Impact.Neutral => 0,
                Impact.Harmful => 1,
                Impact.Beneficial => -1,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return Effects.Average(effect => ImpactValue(effect.Impact) * effect.Evaluate(actor, target));
        }

        public float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions)
        {
            return Effects.Average(e => e.Evaluate(actor, positions));
        }

        public float EvaluatePrice()
        {
            return Effects.Average(effect => effect.EvaluatePrice());
        }

        public string Info()
        {
            return "何らかの効果を発動\n";
        }
    }
}