#nullable enable
using Domain.Model.Effect;

namespace Domain.Service.Effect
{
    public record SpawnEffectSkillResult : ISkillResult
    {
        public SkillResult Result { get; init; }

        private SpawnEffectSkillResult(SkillResult result)
        {
            Result = result;
        }

        public static readonly SpawnEffectSkillResult Failed = new(SkillResult.Failed);
        public static readonly SpawnEffectSkillResult Cancelled = new(SkillResult.Cancelled);
        public static readonly SpawnEffectSkillResult Success = new(SkillResult.Success);
    }
}