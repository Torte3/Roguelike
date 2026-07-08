#nullable enable
using Domain.Model.Character;
using Domain.Model.Item;

namespace Domain.Service.Items
{
    internal sealed class InteractableInventoryRecorder : IInventoryRecorder
    {
        private readonly ICharacter _holder;

        public InteractableInventoryRecorder(ICharacter holder)
        {
            _holder = holder;
        }

        public void RowInserted(int index, IReadOnlyItem item)
        {
            _holder.RecordInteractableChange();
        }

        public void RowRemoved(int index)
        {
            _holder.RecordInteractableChange();
        }

        public void RowsSwapped(int firstIndex, int secondIndex)
        {
        }
    }
}
