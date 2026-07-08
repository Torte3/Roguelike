#nullable enable
using UnityEngine;
using View.UI.Inventory;

namespace View.UI.Menus
{
    internal sealed class LibraryItemRow : IItemRow
    {
        public LibraryItemRow(Sprite icon, bool isShiny, string info)
        {
            Icon = icon;
            IsShiny = isShiny;
            Info = info;
        }

        public string Name => "";
        public bool IsUsable => true;
        public Sprite Icon { get; }
        public bool CanSelect => false;
        public int? Count => null;
        public bool HasEquippedBadge => false;
        public bool IsCursed => false;
        public bool IsShiny { get; }
        public bool IsCountIdentified => true;
        public bool IsCurseIdentified => true;
        public string Info { get; }
    }
}
