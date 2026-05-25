#nullable enable
using Domain.Model.Item;
using Domain.Model.Map;
using View.UI;

namespace Provider
{
    public static class ItemPreviewViewDataBuilder
    {
        public static ItemPreviewViewData Build(IMap map, IItem item, bool assumeIdentified = false)
        {
            var player = map.Player;
            var isIdentified = assumeIdentified || player.Character.IsKnownItem(item);
            var name = isIdentified
                ? item.RevealedName
                : item.GetName(player, map.ItemPlaceholders);
            int? count = item.IsEquipped.IsNone && item.HasActivatableSkill
                ? item.RemainingUses.CurrentValue
                : null;
            var info = isIdentified
                ? item.FullInfo()
                : item.Info(player, map.ItemPlaceholders);

            return new ItemPreviewViewData(
                name,
                item.Icon,
                count,
                item.IsEquipped.UnwrapOr(false),
                item.IsCursed,
                item.IsShiny,
                isIdentified,
                item.IsCurseIdentified,
                info
            );
        }
    }
}
