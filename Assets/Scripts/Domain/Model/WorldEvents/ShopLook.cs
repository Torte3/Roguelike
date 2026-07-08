#nullable enable
using Domain.Model.Entity;
using Utilities;

namespace Domain.Model.WorldEvents
{
    public record ShopLook(int PurchasePrice, int SalePrice, Id<IEntity> Clerk, bool ClerkAppearsInteractable);
}
