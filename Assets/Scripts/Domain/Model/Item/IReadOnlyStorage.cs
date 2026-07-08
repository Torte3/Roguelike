#nullable enable
using System.Collections.Generic;

namespace Domain.Model.Item
{
    public interface IReadOnlyStorage
    {
        public IEnumerable<IReadOnlyItem> AllItems { get; }
        public IEnumerable<(IReadOnlyItem Item, int Index)> AllItemsWithIndex { get; }
        public bool CanRemoveItem { get; }
    }
}
