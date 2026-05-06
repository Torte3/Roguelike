using System;
using System.Collections.Generic;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

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

        public string Info()
        {
            var info = "";

            if (Cost > 0)
                info += $"消費HP: {ItemDescriptionRichText.RichHpCost(Cost)}\n";

            if (RushDistance > 0)
                info += $"最初に{ItemDescriptionRichText.RichSpatial(RushDistance)}マス前に進む\n";

            var positionInfo = Position.Info();
            var areaInfo = Area.Info();
            info += EffectTargetDescription.OnUse(positionInfo, areaInfo, useOrThrowCombinedTargets: false) + "\n";
            foreach (var (effect, index) in Effects.Index())
            {
                info += ItemDescriptionRichText.StyleEffectInfo(effect, effect.Info());
            }
            if (Repeats > 1)
                info += $"効果は{ItemDescriptionRichText.RichMeta(Repeats)}回発動する\n";
            info += ItemDescriptionRichText.ColorPercentagesInPlainText($"成功率：{ProbabilityOfSuccess:P0}\n");

            if (BackStepDistance > 0)
                info += $"最後に{ItemDescriptionRichText.RichSpatial(BackStepDistance)}マス後ろに下がる\n";

            if (ChargeTurn > 0)
                info += $"発動には{ItemDescriptionRichText.RichTurns(ChargeTurn)}ターンかかる\n";

            if (CoolTime > 0)
                info += $"発動後に{ItemDescriptionRichText.RichTurns(CoolTime)}ターンは再使用不能\n";
            return info;
        }
    }
}