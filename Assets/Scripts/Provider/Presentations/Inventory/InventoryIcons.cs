#nullable enable
using UnityEngine;
using Utilities;

namespace Provider.Presentations.Inventory
{
    public sealed class InventoryIcons
    {
        private const string UnderfootIconName = "icons_full_16_751";
        private const string BareHandIconName = "icons_full_16_222";

        public InventoryIcons()
        {
            Underfoot = ObjectLoader.LoadIcon(UnderfootIconName);
            BareHand = ObjectLoader.LoadIcon(BareHandIconName);
        }

        internal Sprite Underfoot { get; }
        internal Sprite BareHand { get; }
    }
}
