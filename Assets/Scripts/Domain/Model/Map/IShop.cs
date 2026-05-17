#nullable enable
using R3;

namespace Domain.Model.Map
{
    public interface IShop
    {
        public ReadOnlyReactiveProperty<bool> IsInside { get; }
        public ReadOnlyReactiveProperty<bool> IsStolen { get; }
        public int GetPurchasePrice(IMap map);
        public int GetSalePrice(IMap map);
    }
}
