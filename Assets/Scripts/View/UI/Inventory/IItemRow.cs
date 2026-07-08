#nullable enable
using UnityEngine;

namespace View.UI.Inventory
{
    public interface IItemRow
    {
        public string Name { get; }
        public bool IsUsable { get; }
        public Sprite Icon { get; }
        public bool CanSelect { get; }
        public int? Count { get; }
        public bool HasEquippedBadge { get; }
        public bool IsCursed { get; }
        public bool IsShiny { get; }
        public bool IsCountIdentified { get; }
        public bool IsCurseIdentified { get; }
        public string Info { get; }
    }
}
