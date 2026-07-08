#nullable enable
namespace Domain.Model.Item.Description
{
    public record ItemDescription(
        ItemState State,
        ItemName Name,
        EquipState EquipState,
        int RemainingUses,
        int MaxUses,
        int UpgradeCount,
        int UpgradeLimit,
        bool IsCursed,
        bool IsCurseIdentified,
        bool CanUse,
        bool CanThrow,
        string Details,
        ItemAbilities? Abilities);
}
