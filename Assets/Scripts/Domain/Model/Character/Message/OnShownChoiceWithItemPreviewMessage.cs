#nullable enable
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Model.Character.Message
{
    public record OnShownChoiceWithItemPreviewMessage(
        ChoiceMessage? Message,
        IReadOnlyMap Map,
        IReadOnlyItem[] Items,
        int? CancelChoiceIndex = null);
}
