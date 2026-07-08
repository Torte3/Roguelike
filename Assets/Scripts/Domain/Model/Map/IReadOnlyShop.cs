#nullable enable
using Domain.Model.Entity;
using Domain.Model.Item;
using R3;
using Utilities;

namespace Domain.Model.Map
{
    public interface IReadOnlyShop
    {
        public ReadOnlyReactiveProperty<bool> IsInside { get; }
        public Id<IEntity> ClerkId { get; }
        public bool ClerkAppearsInteractable(IMap map);
        public int GetPurchasePrice(IReadOnlyMap map);
        public int GetSalePrice(IReadOnlyMap map);
        public int GetPrice(IReadOnlyItem item, IReadOnlyMap map);
    }
}
