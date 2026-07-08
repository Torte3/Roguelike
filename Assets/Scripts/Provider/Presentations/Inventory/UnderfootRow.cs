#nullable enable
using UnityEngine;
using View.UI.Inventory;

namespace Provider.Presentations.Inventory
{
    internal sealed class UnderfootRow : IItemRow
    {
        internal const string Heading = "[足元]";

        private readonly IItemRow _item;

        public UnderfootRow(IItemRow item)
        {
            _item = item;
        }

        public string Name => $"{Heading} {_item.Name}";
        public bool IsUsable => _item.IsUsable;
        public Sprite Icon => _item.Icon;
        public bool CanSelect => _item.CanSelect;
        public int? Count => _item.Count;
        public bool HasEquippedBadge => _item.HasEquippedBadge;
        public bool IsCursed => _item.IsCursed;
        public bool IsShiny => _item.IsShiny;
        public bool IsCountIdentified => _item.IsCountIdentified;
        public bool IsCurseIdentified => _item.IsCurseIdentified;
        public string Info => _item.Info;
    }
}
