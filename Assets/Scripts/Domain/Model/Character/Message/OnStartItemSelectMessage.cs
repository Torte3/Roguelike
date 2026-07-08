using Domain.Model.Item;
using Domain.Model.Item.Slots;

namespace Domain.Model.Character.Message
{
    public record ItemSelectPreview(ItemFocus Focus, IReadOnlyItem Item, string? Note);

    public record OnStartItemSelectMessage(
        string Text,
        ItemFocus[] DisabledItemIndexes,
        ItemSelectPreview[]? Previews = null,
        ItemSelectPreview? DefaultPreview = null,
        string PreviewTitle = ""
    );
}