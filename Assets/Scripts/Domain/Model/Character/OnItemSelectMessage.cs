using Domain.Model.Item;

namespace Domain.Model.Character
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