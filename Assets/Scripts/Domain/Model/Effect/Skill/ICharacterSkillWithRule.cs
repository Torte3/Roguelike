namespace Domain.Model.Effect.Skill
{
    public interface ICharacterSkillWithRule
    {
        public ISkillWithCost Skill { get; }
        public int Priority { get; }
    }
}