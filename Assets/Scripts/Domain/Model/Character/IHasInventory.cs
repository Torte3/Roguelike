#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Character.Message;
using Domain.Model.Item;
using Domain.Model.Item.Slots;
using Domain.Model.Map;
using Domain.Model.WorldEvents;

namespace Domain.Model.Character
{
    public interface IHasInventory : IItemKnowledge
    {
        public IInventory Inventory { get; }
        public void KnowItem(IItem item, DiscoveryCause cause);
        public bool IsCurseKnown(IItem item);
        public UniTask<int?> SelectItem(string text, params int[] disabledItems);
        public UniTask<int?> SelectItemWithCanSelect(string text, Func<IItem, bool> canSelect);
        public UniTask<int?> SelectItemWithCanSelectPreview(string text, Func<IItem, bool> canSelect, Func<IItem, ItemSelectPreview?> buildPreview, ItemSelectPreview? defaultPreview, string previewTitle);
        public UniTask<ItemFocus> SelectItemContainsGroundItem(string text, params ItemFocus[] disabledItems);
        public UniTask<ItemFocus> SelectItemWithCanSelectContainsGroundItem(string text, IPlayer player, IMap map, Func<IItem, bool> canSelect);
    }
}