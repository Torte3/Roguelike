#nullable enable
using Domain.Model.Effect;

namespace Domain.Service.Effect
{
    public record InventoryTargetSkillResult : ISkillResult
    {
        public SkillResult Result { get; init; }

        private InventoryTargetSkillResult(SkillResult result)
        {
            Result = result;
        }

        public static readonly InventoryTargetSkillResult Failed = new(SkillResult.Failed);
        public static readonly InventoryTargetSkillResult Cancelled = new(SkillResult.Cancelled);
        public static readonly InventoryTargetSkillResult Success = new(SkillResult.Success);
    }
}