#nullable enable
using Domain.Model.Character;

namespace Domain.Model.WorldEvents
{
    public record MimicRevealed(bool IsVisible, EntityLabel Disguise, string MimicName, InventoryLook? Inventory)
        : WorldEvent(IsVisible), IInventoryEvent;
    public record DeviceBroken(bool IsVisible, string DeviceName) : WorldEvent(IsVisible);
    public record MonsterHouseEntered : AlwaysVisibleEvent;
    public record OminousPresenceFelt : AlwaysVisibleEvent;
    public record TheftDetected(InventoryLook? Inventory, Underfoot? Underfoot, ShopLook Shop)
        : AlwaysVisibleEvent, IInventoryEvent, IUnderfootEvent;
    public record ShopEntered(InventoryLook? Inventory, Underfoot? Underfoot, ShopLook? Shop) : AlwaysVisibleEvent, IInventoryEvent, IUnderfootEvent, IShopEvent;
    public record ShopExited(InventoryLook? Inventory, Underfoot? Underfoot, ShopLook Shop)
        : AlwaysVisibleEvent, IInventoryEvent, IUnderfootEvent;

    public record ShopSettled(CharacterLabel Label, int Received, int Paid, int Money, InventoryLook? Inventory, Underfoot? Underfoot, ShopLook? Shop)
        : AlwaysVisibleEvent, IInventoryEvent, IUnderfootEvent, IShopEvent;
    public record ShopRoomItemsChanged(ShopLook? Shop) : AlwaysVisibleEvent, IShopEvent;
    public record ShopPaymentRefused(CharacterLabel Label, int Shortfall) : AlwaysVisibleEvent;

    public record FacilityUsed(FixtureKind Kind) : AlwaysVisibleEvent;
    public record DebugMessage(string Message) : AlwaysVisibleEvent;
}
