#nullable enable
using Domain.Model.Effect;
using Domain.Model.Effect.Skill;

namespace Domain.Service.Effect
{
    public record ItemTargetSkillResult : ISkillResult
    {
        public SkillResult Result { get; init; }

        private ItemTargetSkillResult(SkillResult result)
        {
            Result = result;
        }

        public static readonly ItemTargetSkillResult Failed = new(SkillResult.Failed);
        public static readonly ItemTargetSkillResult Cancelled = new(SkillResult.Cancelled);
        public static readonly ItemTargetSkillResult Success = new(SkillResult.Success);
    }
}