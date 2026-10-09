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
    public class SkillDataOnThrow : ISkillData
    {
        public IEffectPosition Position => new AtFeet();

        [field: SerializeReference]
        [field: Required]
        public IArea Area { get; private set; }

        [field: SerializeReference]
        [field: Required]
        public List<IEffect> Effects { get; private set; }

        public int Repeats => 1;

        [field: SerializeField]
        [field: Range(0, 1)]
        public float ProbabilityOfSuccess { get; private set; } =
            CommonSenseParameters.SkillOnThrowProbabilityOfSuccess;

        public int Cost => 0;
        public int RushDistance => 0;
        public int BackStepDistance => 0;
        public int ChargeTurn => 0;
        public int CoolTime => 0;

        public string Log => "";

        public SkillDataOnThrow(IArea area, List<IEffect> effect, float probabilityOfSuccess)
        {
            Area = area;
            Effects = effect;
            ProbabilityOfSuccess = probabilityOfSuccess;
        }

#if UNITY_EDITOR
        internal void SetSameEffect(SkillDataOnUse skillDataOnUse)
        {
            Effects = skillDataOnUse.Effects;
        }

        public void OnValidate()
        {
            if (ProbabilityOfSuccess == 0 ||
                ProbabilityOfSuccess == CommonSenseParameters.SkillOnUseProbabilityOfSuccess)
            {
                ProbabilityOfSuccess = CommonSenseParameters.SkillOnThrowProbabilityOfSuccess;
            }

            if (ProbabilityOfSuccess == 0.8f)
            {
                ProbabilityOfSuccess = CommonSenseParameters.SkillOnThrowProbabilityOfSuccess;
            }
        }
#endif
    }
}