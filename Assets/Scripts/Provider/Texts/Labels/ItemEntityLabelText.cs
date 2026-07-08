#nullable enable
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal sealed class ItemEntityLabelText : EntityLabelText
    {
        private readonly ItemEntityLabel _label;

        public ItemEntityLabelText(ItemEntityLabel label)
        {
            _label = label;
        }

        public override string Text => new ItemNameText(_label.Item).Text;
    }
}
