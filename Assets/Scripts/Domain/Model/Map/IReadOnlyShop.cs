#nullable enable
using Domain.Model.Item;
using R3;

namespace Domain.Model.Map
{
    public interface IReadOnlyShop
    {
        public ReadOnlyReactiveProperty<bool> IsInside { get; }
        public int GetPurchasePrice(IReadOnlyMap map);
        public int GetSalePrice(IReadOnlyMap map);
        public int GetPrice(IReadOnlyItem item, IReadOnlyMap map);
    }
}
