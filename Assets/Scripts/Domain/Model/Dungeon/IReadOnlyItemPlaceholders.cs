#nullable enable
using Domain.Model.Item;

namespace Domain.Model.Dungeon
{
    public interface IReadOnlyItemPlaceholders
    {
        public string GetPlaceholder(string baseName, ItemCategory category);
    }
}
