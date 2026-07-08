#nullable enable
using Domain.Model.Item;
using Domain.Model.Map;
using Provider.Texts;
using View.UI;

namespace Provider
{
    public static class ItemPreviewViewDataBuilder
    {
        public static ItemPreviewViewData Build(IReadOnlyMap map, IReadOnlyItem item, bool assumeIdentified = false)
        {
            var player = map.PlayerCharacter;
            var isIdentified = assumeIdentified || player.IsKnownItem(item);
            var name = isIdentified
                ? item.RevealedName
                : ItemNameText.Of(item.NameIn(map));
            int? count = item.IsEquipped.IsNone && item.HasActivatableSkill
                ? item.RemainingUses.CurrentValue
                : null;
            var info = isIdentified
                ? ItemDescriptionText.Of(item.DescribeIdentified())
                : ItemDescriptionText.Of(item.Describe(player, map.ItemPlaceholders));

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
