#nullable enable
using Domain.Model.Item;

namespace Domain.Service.Items
{
    internal interface IInventoryRecorder
    {
        public void RowInserted(int index, IReadOnlyItem item);
        public void RowRemoved(int index);
        public void RowsSwapped(int firstIndex, int secondIndex);
    }
}
