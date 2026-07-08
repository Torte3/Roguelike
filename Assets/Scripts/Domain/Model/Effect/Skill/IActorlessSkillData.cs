using System.Collections.Generic;
using System.Linq;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;

namespace Domain.Model.Effect.Skill
{
    public interface IActorlessSkillData : ISkillData
    {
        public new INotDirectionalArea Area { get; }
        IArea ISkillData.Area => Area;
        public new List<IActorlessEffect> Effects { get; }
        List<IEffect> ISkillData.Effects => Effects.Cast<IEffect>().ToList();
        public new IPositionOnlyDependentEffectPosition Position { get; }
        IEffectPosition ISkillData.Position => Position;
        public new int RushDistance => 0;
        public new int BackStepDistance => 0;
        public new int ChargeTurn => 0;
        public new int CoolTime => 0;
    }
}