#nullable enable

namespace Domain.Model.Item
{
    public record ItemName(bool IsIdentified, string? CustomName, string? RevealedName, string? Placeholder, int UpgradeCount);
}
