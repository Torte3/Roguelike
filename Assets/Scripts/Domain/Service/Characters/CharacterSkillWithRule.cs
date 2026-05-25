#nullable enable
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Memento;
using Domain.Service.Effect;

namespace Domain.Service.Characters
{
    public class CharacterSkillWithRule : ICharacterSkillWithRule
    {
        private readonly SkillWithCost _skill;
        public ISkillWithCost Skill => _skill;
        public int Priority { get; }
        public CharacterSkillWithRule(CharacterSkillWithRuleMemento data)
        {
            _skill = new SkillWithCost(data.Skill);
            Priority = data.Priority;
        }
        public CharacterSkillWithRuleMemento Serialize()
        {
            return new CharacterSkillWithRuleMemento(
                _skill.Serialize(),
                Priority
            );
        }
        public static CharacterSkillWithRuleMemento Build(CharacterSkillWithRuleData data)
        {
            return new CharacterSkillWithRuleMemento(
                SkillWithCost.Build(data.Skill),
                data.Priority
            );
        }
    }
}