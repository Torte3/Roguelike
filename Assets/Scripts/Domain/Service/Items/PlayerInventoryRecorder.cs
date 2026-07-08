#nullable enable
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.WorldEvents;

namespace Domain.Service.Items
{
    internal sealed class PlayerInventoryRecorder : IInventoryRecorder
    {
        private readonly EntityBase _player;
        private readonly IMap _map;

        public PlayerInventoryRecorder(EntityBase player, IMap map)
        {
            _player = player;
            _map = map;
        }

        public void RowInserted(int index, IReadOnlyItem item)
        {
            Record(new InventoryRowInserted(index, item.LookIn(_map)));
        }

        public void RowRemoved(int index)
        {
            Record(new InventoryRowRemoved(index));
        }

        public void RowsSwapped(int firstIndex, int secondIndex)
        {
            _player.Record(new ItemsReordered(_player.Ref, firstIndex, secondIndex));
        }

        private void Record(params InventoryRowChange[] changes)
        {
            _player.Record(new InventoryRowsChanged(_player.Ref, changes));
            _map.RecordShopChanges();
        }
    }
}
