#nullable enable
using Domain.Model.Map;
using Domain.Model.WorldEvents;

namespace Domain.Model.Item
{
    public static class ItemNameExtensions
    {
        public static ItemName NameIn(this IReadOnlyItem item, IReadOnlyMap map)
        {
            return item.GetName(map.PlayerCharacter, map.ItemPlaceholders);
        }

        public static ItemEntityLabel LabelIn(this IReadOnlyItem item, IReadOnlyMap map)
        {
            return new ItemEntityLabel(item.NameIn(map));
        }
    }
}
