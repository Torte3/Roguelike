#nullable enable
using UnityEngine;
using View.UI.Inventory;

namespace Provider.Presentations.Inventory
{
    internal sealed class EmptyRow : IItemRow
    {
        public EmptyRow(string name, Sprite icon, string info)
        {
            Name = name;
            Icon = icon;
            Info = info;
        }

        public string Name { get; }
        public bool IsUsable => false;
        public Sprite Icon { get; }
        public bool CanSelect => true;
        public int? Count => null;
        public bool HasEquippedBadge => false;
        public bool IsCursed => false;
        public bool IsShiny => false;
        public bool IsCountIdentified => true;
        public bool IsCurseIdentified => true;
        public string Info { get; }
    }
}
