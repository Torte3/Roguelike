#nullable enable
using System.Collections.Generic;
using System.Text;

namespace Domain.Model.Item
{
    public enum CurseKnowledge
    {
        Unknown,
        Cursed,
        NotCursed,
    }

    public enum ItemAbilityKind
    {
        Features,
        PassiveSkills,
    }

    public record ItemAbilities(ItemAbilityKind Kind, int Count, int Limit, IReadOnlyList<string> Names)
    {
        protected virtual bool PrintMembers(StringBuilder builder)
        {
            builder.Append($"Kind = {Kind}, Count = {Count}, Limit = {Limit}, Names = {string.Join(", ", Names)}");
            return true;
        }
    }

    public record ItemDescription(
        ItemState State,
        ItemName Name,
        bool? IsEquipped,
        int RemainingUses,
        int MaxUses,
        int UpgradeCount,
        int UpgradeLimit,
        CurseKnowledge Curse,
        bool CanUse,
        bool CanThrow,
        string? Details,
        ItemAbilities? Abilities);
}
