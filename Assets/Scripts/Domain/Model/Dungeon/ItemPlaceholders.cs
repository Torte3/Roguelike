using System.Collections.Generic;
using Domain.Model.Item;
using Domain.Model.Memento;

namespace Domain.Model.Dungeon
{
    public class ItemPlaceholders : ISerializable<ItemPlaceholdersMemento>, IReadOnlyItemPlaceholders
    {
        private Placeholders _placeholderData;
        private Dictionary<string, string> _placeholders = new();
        private Dictionary<string, string> _playerAssignedNames = new();
        private PlaceholderIndexes _potionPlaceholderIndexes;
        private PlaceholderIndexes _scrollPlaceholderIndexes;
        private PlaceholderIndexes _bookPlaceholderIndexes;
        private PlaceholderIndexes _wandPlaceholderIndexes;
        private PlaceholderIndexes _artifactPlaceholderIndexes;

        public ItemPlaceholders(ItemPlaceholdersMemento memento, Placeholders placeholders)
        {
            _placeholderData = placeholders;
            _placeholders = memento.Placeholders;
            _playerAssignedNames = memento.PlayerAssignedNames;
            _potionPlaceholderIndexes = new PlaceholderIndexes(placeholders.PotionPlaceholders, memento.PotionUsedPlaceholderIndexes);
            _scrollPlaceholderIndexes = new PlaceholderIndexes(placeholders.ScrollPlaceholders, memento.ScrollUsedPlaceholderIndexes);
            _bookPlaceholderIndexes = new PlaceholderIndexes(placeholders.BookPlaceholders, memento.BookUsedPlaceholderIndexes);
            _wandPlaceholderIndexes = new PlaceholderIndexes(placeholders.WandPlaceholders, memento.WandUsedPlaceholderIndexes);
            _artifactPlaceholderIndexes = new PlaceholderIndexes(placeholders.ArtifactPlaceholders, memento.ArtifactUsedPlaceholderIndexes);
        }

        public ItemPlaceholdersMemento Serialize()
        {
            return new ItemPlaceholdersMemento(
                _placeholders,
                _playerAssignedNames,
                _potionPlaceholderIndexes.UsedIndexes,
                _scrollPlaceholderIndexes.UsedIndexes,
                _bookPlaceholderIndexes.UsedIndexes,
                _wandPlaceholderIndexes.UsedIndexes,
                _artifactPlaceholderIndexes.UsedIndexes
            );
        }

        public static ItemPlaceholdersMemento Build()
        {
            return new ItemPlaceholdersMemento(
                new Dictionary<string, string>(),
                new Dictionary<string, string>(),
                new List<int>(),
                new List<int>(),
                new List<int>(),
                new List<int>(),
                new List<int>()
            );
        }

        public string GetPlaceholder(ItemData item)
        {
            return GetPlaceholder(item.name, item.Category);
        }

        public string GetPlaceholder(string baseName, ItemCategory category)
        {
            if (_playerAssignedNames.ContainsKey(baseName))
                return _playerAssignedNames[baseName];
            if (!_placeholders.ContainsKey(baseName))
            {
                var placeholder = category switch
                {
                    ItemCategory.Potions => _potionPlaceholderIndexes.GetAtRandomAndRemove(_placeholderData
                        .PotionPlaceholders),
                    ItemCategory.Scrolls => _scrollPlaceholderIndexes.GetAtRandomAndRemove(_placeholderData
                        .ScrollPlaceholders),
                    ItemCategory.Books => _bookPlaceholderIndexes.GetAtRandomAndRemove(
                        _placeholderData.BookPlaceholders),
                    ItemCategory.Wands => _wandPlaceholderIndexes.GetAtRandomAndRemove(
                        _placeholderData.WandPlaceholders),
                    ItemCategory.Artifacts => _artifactPlaceholderIndexes.GetAtRandomAndRemove(_placeholderData
                        .ArtifactPlaceholders),
                    _ => baseName
                };
                _placeholders[baseName] = placeholder;
            }

            return _placeholders[baseName];
        }

        public void Rename(string baseName, string newName)
        {
            if (newName == "")
                return;
            _playerAssignedNames[baseName] = newName;
        }

        public void ClearPlayerAssignedNames()
        {
            _playerAssignedNames.Clear();
        }
    }
}