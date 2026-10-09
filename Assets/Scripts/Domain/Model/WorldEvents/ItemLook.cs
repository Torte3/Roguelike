#nullable enable
using Domain.Model.Item;
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
        int? Count,
        bool IsEquipped,
        bool IsCursed,
        bool IsShiny,
        bool IsKnown,
        bool IsCurseIdentified,
        ShopPrice? Price);
}
