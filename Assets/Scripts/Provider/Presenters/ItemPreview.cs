#nullable enable
using Domain.Model.Item;
using Domain.Model.Map;
using Provider.Texts;
using View.UI.ItemPreview;

namespace Provider.Presenters
{
    internal sealed class ItemPreview
    {
        private readonly IReadOnlyMap _map;
        private readonly IReadOnlyItem _item;
        private readonly bool _assumesIdentified;

        public ItemPreview(IReadOnlyMap map, IReadOnlyItem item, bool assumesIdentified = false)
        {
            _map = map;
            _item = item;
            _assumesIdentified = assumesIdentified;
        }

        public ItemPreviewViewData ViewData
        {
            get
            {
                var description = _assumesIdentified
                    ? _item.DescribeIdentified()
                    : _item.Describe(_map.PlayerCharacter, _map.ItemPlaceholders);
                int? count = _item.IsEquipped.IsNone && _item.HasActivatableSkill
                    ? _item.RemainingUses.CurrentValue
                    : null;

                return new ItemPreviewViewData(
                    new ItemNameText(description.Name).Text,
                    _item.Icon,
                    count,
                    _item.IsEquipped.UnwrapOr(false),
                    _item.IsCursed,
                    _item.IsShiny,
                    description.Name.IsIdentified,
                    _item.IsCurseIdentified,
                    new ItemDescriptionText(description).Text
                );
            }
        }
    }
}
