#nullable enable
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal sealed class ItemEntityLabelText : EntityLabelText<ItemEntityLabel>
    {
        protected override string Of(ItemEntityLabel label) => ItemNameText.Of(label.Item);
    }
}
