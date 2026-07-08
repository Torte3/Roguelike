using System.Collections.Generic;
using Utilities;

namespace Domain.Model.Dungeon
{
    internal class PlaceholderIndexes
    {
        private List<int> _prefixIndexes = new();
        private List<int> _placeholderIndexes = new();
        public readonly List<int> UsedIndexes;

        public PlaceholderIndexes(CategoryPlaceholders placeholders, List<int> usedIndexes)
        {
            UsedIndexes = usedIndexes;
            if (placeholders._placeholderPrefixes.Count == 0)
            {
                for (var i = 0; i < placeholders._placeholders.Count; i++)
                {
                    _prefixIndexes.Add(-1);
                    _placeholderIndexes.Add(i);
                }
            }
            else
            {
                for (var i = 0; i < placeholders._placeholderPrefixes.Count; i++)
                {
                    for (var j = 0; j < placeholders._placeholders.Count; j++)
                    {
                        _prefixIndexes.Add(i);
                        _placeholderIndexes.Add(j);
                    }
                }
            }
        }

        public string GetAtRandomAndRemove(CategoryPlaceholders placeholders)
        {
            var index = RandUtils.RangeWithoutExcludes(_placeholderIndexes.Count, UsedIndexes.ToArray());
            UsedIndexes.Add(index);

            var prefixIndex = _prefixIndexes[index];
            var placeholderIndex = _placeholderIndexes[index];

            var prefix = prefixIndex != -1 ? placeholders._placeholderPrefixes[prefixIndex] : "";
            return $"{prefix}{placeholders._placeholders[placeholderIndex]}{placeholders._placeholderSuffix}";
        }
    }
}