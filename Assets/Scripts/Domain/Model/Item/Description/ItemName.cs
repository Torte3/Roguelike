#nullable enable
namespace Domain.Model.Item.Description
{
    public sealed record ItemName(
        string RevealedName,
        int UpgradeCount,
        bool IsIdentified,
        string? CustomName,
        string? Placeholder);
}
