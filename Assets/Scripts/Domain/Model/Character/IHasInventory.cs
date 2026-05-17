#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Model.Character
{
    public interface IHasInventory
    {
        public IInventory Inventory { get; }
        public void KnowItem(IItem item, bool log);
        public void KnowCurse(IItem item, bool log);
        public bool IsKnownItem(IItem item);
        public bool IsCurseKnown(IItem item);
        public UniTask<int?> SelectItem(string text, params int[] disabledItems);
        public UniTask<int?> SelectItemWithCanSelect(string text, Func<IItem, bool> canSelect);
        public UniTask<int?> SelectItemWithCanSelectPreview(string text, Func<IItem, bool> canSelect, Func<IItem, ItemSelectPreview?> buildPreview, ItemSelectPreview? defaultPreview, string previewTitle);
        public UniTask<ItemFocus> SelectItemContainsGroundItem(string text, params ItemFocus[] disabledItems);
        public UniTask<ItemFocus> SelectItemWithCanSelectContainsGroundItem(string text, IPlayer player, IMap map, Func<IItem, bool> canSelect);
    }
}