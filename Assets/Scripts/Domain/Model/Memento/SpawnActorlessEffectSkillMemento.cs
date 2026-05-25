#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class SpawnActorlessEffectSkillMemento : ISkillMemento
    {
        [field: SerializeReference] public IPositionOnlyDependentEffectPosition Position { get; private set; }
        [field: SerializeReference] public INotDirectionalArea Area { get; private set; }
        [field: SerializeReference] public List<IActorlessEffect> Effects { get; private set; }
        [field: SerializeField] public int Repeats { get; private set; }
        [field: SerializeField] public float ProbabilityOfSuccess { get; private set; }
        [field: SerializeField] public string Log { get; private set; }

        public SpawnActorlessEffectSkillMemento(
            IPositionOnlyDependentEffectPosition position,
            INotDirectionalArea area,
            List<IActorlessEffect> effects,
            int repeats,
            float probabilityOfSuccess,
            string log)
        {
            Position = position;
            Area = area;
            Effects = effects;
            Repeats = repeats;
            ProbabilityOfSuccess = probabilityOfSuccess;
            Log = log;
        }

        public SpawnActorlessEffectSkillMemento CopyWith(
            IPositionOnlyDependentEffectPosition? position = null,
            INotDirectionalArea? area = null,
            List<IActorlessEffect>? effect = null,
            int? repeats = null,
            float? probabilityOfSuccess = null,
            string? log = null)
        {
            return new SpawnActorlessEffectSkillMemento(
                position ?? Position,
                area ?? Area,
                effect ?? Effects,
                repeats ?? Repeats,
                probabilityOfSuccess ?? ProbabilityOfSuccess,
                log ?? Log
            );
        }
    }
}