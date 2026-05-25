#nullable enable
using Domain.Model.Item;

namespace Domain.Model.Character.Message
{
    public record OnItemInserted(IItem NewItem, int Index);
    public record OnItemRemoved(IItem OldItem, int Index);
    public record OnItemReplaced(IItem NewItem, IItem OldItem, int Index);
    public record OnItemUpdated(IItem Item);
}