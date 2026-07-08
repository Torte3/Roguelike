#nullable enable
using Domain.Model.Effect;
using Utilities.Result;

namespace Domain.Service.Effect
{
    internal record SkillOutcome : ISkillResult
    {
        public SkillResult Result { get; }

        private SkillOutcome(SkillResult result)
        {
            Result = result;
        }

        public static readonly SkillOutcome Success = new(SkillResult.Success);
        public static readonly SkillOutcome Failed = new(SkillResult.Failed);
        public static readonly SkillOutcome Cancelled = new(SkillResult.Cancelled);

        public static SkillOutcome NotRun<T, TFailure>(Result<T, TFailure> result)
        {
            return result.IsCancelled ? Cancelled : Failed;
        }
    }
}
