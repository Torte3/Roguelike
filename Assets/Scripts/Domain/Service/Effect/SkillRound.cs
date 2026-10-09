#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using UnityEngine;

namespace Domain.Service.Effect
{
    internal sealed class SkillRound
    {
        private readonly List<EffectHit> _hits = new();

        public void Hit(ICharacter target, IEffect effect)
        {
            _hits.Add(new EffectHit(target.Entity.Ref, effect.AppliedImpact));
        }

        public void Record(IMap map, IReadOnlyList<Vector2Int> area, Color color)
        {
            map.Events.Record(new AreaEffectApplied(Visibility.Within(map, area), area, color, _hits));
        }
    }
}
