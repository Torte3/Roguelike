#nullable enable
using Domain.Model.Map;

namespace Domain.Model.Item.Slots
{
    public sealed class GroundItemSlot : IItemSlot
    {
        private readonly IItemEntity _itemEntity;
        private readonly IMap _map;

        public GroundItemSlot(IItemEntity itemEntity, IMap map)
        {
            _itemEntity = itemEntity;
            _map = map;
        }

        public IItem Item => _itemEntity.Item;

        public void Replace(IItem item)
        {
            var position = _itemEntity.Entity.CurrentPosition;
            _itemEntity.Entity.Destroy();
            _map.SpawnItem(item, position);
        }
    }
}
