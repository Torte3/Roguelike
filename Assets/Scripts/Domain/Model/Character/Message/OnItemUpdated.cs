#nullable enable
using Domain.Model.Item;

namespace Domain.Model.Character.Message
{
    public record OnItemInserted<TItem>(TItem NewItem, int Index) where TItem : IReadOnlyItem;
    public record OnItemRemoved<TItem>(TItem OldItem, int Index) where TItem : IReadOnlyItem;
    public record OnItemReplaced<TItem>(TItem NewItem, TItem OldItem, int Index) where TItem : IReadOnlyItem;
}