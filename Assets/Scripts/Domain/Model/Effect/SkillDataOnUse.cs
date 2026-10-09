using System;
using System.Collections.Generic;
using Domain.Model.Effect.Area;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Effect
{
    [Serializable]
    public class SkillDataOnUse : ISkillData
    {
        [field: SerializeReference]
        [field: Required]
        public IEffectPosition Position { get; private set; }

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
        public int Cost { get; private set; } = 0;

        [field: SerializeField]
        [field: MinValue(0)]
        public int RushDistance { get; private set; } = 0;

        [field: SerializeField]
        [field: MinValue(0)]
        public int BackStepDistance { get; private set; } = 0;

        [field: SerializeField]
        [field: MinValue(0)]
        public int ChargeTurn { get; private set; } = 0;


        [field: SerializeField]
        [field: MinValue(0)]
        public int CoolTime { get; private set; } = 0;

        public string Log => "";

        public SkillDataOnUse(
            IEffectPosition position,
            IArea area,
            List<IEffect> effects,
            int repeats,
            float probabilityOfSuccess,
            int cost,
            int rushDistance,
            int backStepDistance,
            int chargeTurn,
            int coolTime)
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
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (Repeats == 0)
            {
                Repeats = 1;
            }

            if (ProbabilityOfSuccess == 0)
            {
                ProbabilityOfSuccess = CommonSenseParameters.SkillOnUseProbabilityOfSuccess;
            }
        }
#endif
    }
}