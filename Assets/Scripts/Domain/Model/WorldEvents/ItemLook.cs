#nullable enable
using Domain.Model.Item;
using Domain.Model.Item.Description;
using UnityEngine;
using Utilities;

namespace Domain.Model.WorldEvents
{
    public record ItemLook(
        Id<IItem> Id,
        ItemName Name,
        ItemDescription Description,
        Sprite Icon,
        bool CanAttemptUseOrThrow,
        bool HasActivatableSkill,
        int RemainingUses,
        EquipState EquipState,
        bool IsCursed,
        bool IsShiny,
        bool IsKnown,
        bool IsCurseIdentified)
    {
        public bool IsKnownCursed => IsCursed && IsCurseIdentified;
    }
}
