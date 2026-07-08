#nullable enable
using Domain.Model.Item.Description;

namespace Provider.Texts
{
    internal sealed class ItemNameText
    {
        private readonly ItemName _name;

        public ItemNameText(ItemName name)
        {
            _name = name;
        }

        public string Text => _name.IsIdentified ? IdentifiedText : $"?{_name.CustomName ?? _name.Placeholder}?";

        private string IdentifiedText => (_name.CustomName ?? _name.RevealedName) + _name.UpgradeCount switch
        {
            0 => "",
            > 0 => $" +{_name.UpgradeCount}",
            _ => $" {_name.UpgradeCount}",
        };
    }
}
