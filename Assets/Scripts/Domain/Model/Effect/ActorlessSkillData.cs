using System;
using System.Collections.Generic;
using Domain.Model.Effect.Area;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Effect
{
    [Serializable]
    public class ActorlessSkillData : IActorlessSkillData
    {
        [field: SerializeReference]
        [field: Required]
        public IPositionOnlyDependentEffectPosition Position { get; private set; }

        [field: SerializeReference]
        [field: Required]
        public INotDirectionalArea Area { get; private set; }

        [field: SerializeReference]
        [field: Required]
        public List<IActorlessEffect> Effects { get; private set; }

        [field: SerializeField]
        [field: MinValue(1)]
        public int Repeats { get; private set; } = 1;

        public int Cost => 0;
        public int ChargeTurn => 0;
        public int RushDistance => 0;
        public int BackStepDistance => 0;
        public int CoolTime => 0;

        [field: SerializeField]
        [field: Range(0, 1)]
        public float ProbabilityOfSuccess { get; private set; } = CommonSenseParameters.SkillOnUseProbabilityOfSuccess;

        public string Log => "";

        public ActorlessSkillData(IPositionOnlyDependentEffectPosition position, INotDirectionalArea area,
            List<IActorlessEffect> effect,
            float probabilityOfSuccess)
        {
            Position = position;
            Area = area;
            Effects = effect;
            ProbabilityOfSuccess = probabilityOfSuccess;
        }

#if UNITY_EDITOR
        public void OnValidate(float probabilityOfSuccess)
        {
            if (ProbabilityOfSuccess == 0)
            {
                ProbabilityOfSuccess = probabilityOfSuccess;
            }
        }
#endif
    }
}