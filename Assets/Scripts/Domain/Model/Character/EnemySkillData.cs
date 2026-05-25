using System;
using Domain.Model.Effect;
using Sirenix.OdinInspector;

namespace Domain.Model.Character
{
    [Serializable]
    public class CharacterSkillData
    {
        [Required] public SkillData Skill;
        [MinValue(0)] public int RushDistance;
        [MinValue(0)] public int BackStepDistance;
        [MinValue(0)] public int ChargeTurn;
        [MinValue(0)] public int CoolTime;
        public CharacterSkillData(SkillData skill, int rushDistance, int backStepDistance, int chargeTurn, int coolTime)
        {
            Skill = skill;
            RushDistance = rushDistance;
            BackStepDistance = backStepDistance;
            ChargeTurn = chargeTurn;
            CoolTime = coolTime;
        }
        public string Info()
        {
            var info = "";
            if (RushDistance > 0)
                info += $"最初に{RushDistance}マス前に進む\n";

            info += Skill.Info();

            if (BackStepDistance > 0)
                info += $"最後に{BackStepDistance}マス後ろに下がる\n";

            if (ChargeTurn > 0)
                info += $"発動には{ChargeTurn}ターンかかる\n";

            if (CoolTime > 0)
                info += $"発動後に{CoolTime}ターンは再使用不能\n";
            return info;
        }
    }
}