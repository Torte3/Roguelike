#nullable enable
using Domain.Model.Item;

namespace Provider.Texts
{
    internal static class ItemNameText
    {
        public static string Of(ItemName name)
        {
            return name.IsIdentified ? Identified(name) : Unidentified(name);
        }

        public static string Unidentified(ItemName name)
        {
            return $"?{name.CustomName ?? name.Placeholder}?";
        }

        private static string Identified(ItemName name)
        {
            return (name.CustomName ?? name.RevealedName) + name.UpgradeCount switch
            {
                0 => "",
                > 0 => $" +{name.UpgradeCount}",
                _ => $" {name.UpgradeCount}",
            };
        }
    }
}
