#nullable enable

namespace Domain.Model.Dungeon
{
    public interface IItemPlaceholderAssigner
    {
        public string GetOrAssignPlaceholder(string baseName, ItemCategory category);
    }
}
