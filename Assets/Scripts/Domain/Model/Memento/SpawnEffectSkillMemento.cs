#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class SpawnEffectSkillMemento : ISkillMemento
    {
        [field: SerializeReference] public IEffectPosition Position { get; private set; }
        [field: SerializeReference] public IArea Area { get; private set; }
        [field: SerializeReference] public List<IEffect> Effects { get; private set; }
        [field: SerializeField] public int Repeats { get; private set; }
        [field: SerializeField] public float ProbabilityOfSuccess { get; private set; }
        [field: SerializeField] public int RushDistance { get; private set; }
        [field: SerializeField] public int BackStepDistance { get; private set; }
        [field: SerializeField] public string Log { get; private set; }


        public SpawnEffectSkillMemento(
            IEffectPosition position,
            IArea area,
            List<IEffect> effects,
            int repeats,
            float probabilityOfSuccess,
            int rushDistance,
            int backStepDistance,
            string log)
        {
            Position = position;
            Area = area;
            Effects = effects;
            Repeats = repeats;
            ProbabilityOfSuccess = probabilityOfSuccess;
            RushDistance = rushDistance;
            BackStepDistance = backStepDistance;
            Log = log;
        }

        public SpawnEffectSkillMemento CopyWith(
            IEffectPosition? position = null,
            IArea? area = null,
            List<IEffect>? effect = null,
            int? repeats = null,
            float? probabilityOfSuccess = null,
            int? rushDistance = null,
            int? backStepDistance = null,
            string? log = null)
        {
            return new SpawnEffectSkillMemento(
                position ?? Position,
                area ?? Area,
                effect ?? Effects,
                repeats ?? Repeats,
                probabilityOfSuccess ?? ProbabilityOfSuccess,
                rushDistance ?? RushDistance,
                backStepDistance ?? BackStepDistance,
                log ?? Log
            );
        }
    }
}