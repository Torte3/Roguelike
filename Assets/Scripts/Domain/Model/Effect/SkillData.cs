using System;
using System.Collections.Generic;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Effect
{
    [Serializable]
    public record SkillData : ISkillData
    {
        [field: SerializeReference]
        [field: Required]
        public IEffectPosition Position { get; private set; } = new AtFeet();

        [field: SerializeReference]
        [field: Required]
        public IArea Area { get; private set; }

        [field: SerializeReference]
        [field: Required]
        public List<IEffect> Effects { get; private set; }

        [field: SerializeField]
        [field: MinValue(1)]
        public int Repeats { get; private set; } = 1;

        [field: SerializeField]
        [field: Range(0, 1)]
        public float ProbabilityOfSuccess { get; private set; } = CommonSenseParameters.SkillOnUseProbabilityOfSuccess;

        [field: SerializeField]
        [field: MinValue(0)]
        public int Cost { get; private set; }

        [field: SerializeField]
        [field: MinValue(0)]
        public int RushDistance { get; private set; }

        [field: SerializeField]
        [field: MinValue(0)]
        public int BackStepDistance { get; private set; }

        [field: SerializeField]
        [field: MinValue(0)]
        public int ChargeTurn { get; private set; }

        [field: SerializeField]
        [field: MinValue(0)]
        public int CoolTime { get; private set; }

        [field: SerializeField]
        [field: Required]
        public string Log { get; private set; } = "は行動した";

        public SkillData(
            IEffectPosition position,
            IArea area,
            List<IEffect> effects,
            int repeats,
            float probabilityOfSuccess,
            int cost,
            int rushDistance,
            int backStepDistance,
            int chargeTurn,
            int coolTime,
            string log)
        {
            Position = position;
            Area = area;
            Effects = effects;
            Repeats = repeats;
            ProbabilityOfSuccess = probabilityOfSuccess;
            Cost = cost;
            RushDistance = rushDistance;
            BackStepDistance = backStepDistance;
            ChargeTurn = chargeTurn;
            CoolTime = coolTime;
            Log = log;
        }

#if UNITY_EDITOR
        public void OnValidate(float probabilityOfSuccess)
        {
            if (Repeats == 0)
            {
                Repeats = 1;
            }

            if (ProbabilityOfSuccess == 0)
            {
                ProbabilityOfSuccess = probabilityOfSuccess;
            }
        }
#endif
    }
}