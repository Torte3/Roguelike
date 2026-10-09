#nullable enable
namespace Domain.Model.WorldEvents
{
    public interface IShopEvent
    {
        public ShopLook? Shop { get; }
    }
}
