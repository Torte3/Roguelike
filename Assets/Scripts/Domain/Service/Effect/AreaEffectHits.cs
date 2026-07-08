#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using UnityEngine;

namespace Domain.Service.Effect
{
    internal sealed class AreaEffectHits
    {
        private readonly List<EffectHit> _hits = new();

        public void Add(ICharacter target, IEffect effect)
        {
            _hits.Add(new EffectHit(target.Entity.Ref, ImpactOf(effect)));
        }

        private static Impact ImpactOf(IEffect effect)
        {
            return effect is RandomEffect { ChosenEffect: { } chosen } ? ImpactOf(chosen) : effect.Impact;
        }

        public void Record(IMap map, IReadOnlyList<Vector2Int> area, Color color)
        {
            map.Events.Record(new AreaEffectApplied(area.Any(map.PlayerCharacter.IsVisible), area, color, _hits));
        }
    }
}
