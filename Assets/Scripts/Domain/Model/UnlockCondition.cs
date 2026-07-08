#nullable enable

namespace Domain.Model
{
    public record UnlockCondition(string Description, int Current, int Required)
    {
        public bool IsUnlocked => Current >= Required;
    }
}
