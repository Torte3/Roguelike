namespace Domain.Model.Effect
{
    public interface ICharacterSkillWithRule
    {
        public ISkillWithCost Skill { get; }
        public int Priority { get; }
    }
}