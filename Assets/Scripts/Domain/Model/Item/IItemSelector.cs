#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Character;

namespace Domain.Model.Item
{
    public interface IItemSelector
    {
        public UniTask<ItemFocus> SelectItem(string text, ItemFocus[] disabledItems);
        public UniTask<ItemFocus> SelectItemWithPreview(string text, ItemFocus[] disabledItems, ItemSelectPreview[] previews, ItemSelectPreview? defaultPreview, string previewTitle);
    }
}