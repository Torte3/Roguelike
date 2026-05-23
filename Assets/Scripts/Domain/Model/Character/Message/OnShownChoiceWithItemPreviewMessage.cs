#nullable enable
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Model.Character.Message
{
    public record OnShownChoiceWithItemPreviewMessage(
        string? Text,
        IMap Map,
        (string Choice, IItem Item)[] Choices,
        int? CancelChoiceIndex = null);
}
