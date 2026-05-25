using System;
using System.Collections.Generic;
using Domain.Model.Effect.Area;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

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

        public string Info()
        {
            var info = "";
            if (Cost > 0)
                info += $"消費HP: {Cost}\n";
            if (Repeats > 1)
                info += $"発動回数: {Repeats}回\n";
            foreach (var (effect, index) in Effects.Index())
            {
                info += $"効果{index + 1}: {effect.Info()}\n";
            }

            info += $"発動位置: {Position.Info()}\n";
            info += $"範囲: {Area.Info()}\n";
            info += $"発動確率: {ProbabilityOfSuccess:P0}";
            if (RushDistance > 0)
                info += $"最初に{RushDistance}マス前に進む\n";
            if (BackStepDistance > 0)
                info += $"最後に{BackStepDistance}マス後ろに下がる\n";
            if (ChargeTurn > 0)
                info += $"発動には{ChargeTurn + 1}ターンかかる\n";
            if (CoolTime > 0)
                info += $"発動後に{CoolTime}ターンは再使用不能\n";
            return info;
        }
    }
}