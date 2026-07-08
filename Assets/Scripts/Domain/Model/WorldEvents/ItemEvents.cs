#nullable enable
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using UnityEngine;

namespace Domain.Model.WorldEvents
{
    public record ObtainedItem(Sprite Icon, Vector2Int Position);

    public interface IItemObtainedEvent
    {
        public ObtainedItem Obtained { get; }
    }

    public record ItemBroken(bool IsVisible, ItemName ItemName) : WorldEvent(IsVisible);
    public record ItemThrown(EntityRef Entity, CharacterLabel Label, ItemName ItemName, Flight Flight, InventoryLook? Inventory,
        Underfoot? Underfoot) : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IFlightEvent, IInventoryEvent, IUnderfootEvent;
    public record ItemDropped(EntityRef Entity, CharacterLabel Label, ItemName ItemName, InventoryLook? Inventory)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IInventoryEvent;
    public record ItemDiscarded(EntityRef Entity, CharacterLabel Label, ItemName ItemName, InventoryLook? Inventory)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IInventoryEvent;
    public record ItemExchanged(EntityRef Entity, CharacterLabel Label, ItemName DroppedName, ItemName PickedUpName,
        ObtainedItem Obtained, InventoryLook? Inventory, Underfoot? Underfoot)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IItemObtainedEvent, IInventoryEvent, IUnderfootEvent;
    public record ItemPickedUp(EntityRef Entity, CharacterLabel Label, ItemName ItemName, bool IsAutomatic, ObtainedItem Obtained,
        InventoryLook? Inventory, Underfoot? Underfoot)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IItemObtainedEvent, IInventoryEvent, IUnderfootEvent;
    public record ItemSteppedOn(EntityRef Entity, CharacterLabel Label, ItemName ItemName) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;
    public record ItemGiven(EntityRef Entity, CharacterLabel Label, ItemName ItemName, InventoryLook? Inventory,
        InventoryLook GiverInventory, ShopLook? Shop) : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IInventoryEvent, IShopEvent;
    public record ItemObtainedFromChest(CharacterLabel Label, ItemName ItemName, ObtainedItem Obtained, InventoryLook? Inventory)
        : AlwaysVisibleEvent, IItemObtainedEvent, IInventoryEvent;
    public record ItemsMerged(CharacterLabel Label, ItemName BaseItemName, ItemName MergedItemName, InventoryLook? Inventory, ShopLook? Shop)
        : AlwaysVisibleEvent, IInventoryEvent, IShopEvent;
    public record MergedItemNotStored : AlwaysVisibleEvent;
    public record ItemIdentified(bool IsVisible, ItemName UnidentifiedName, ItemName IdentifiedName, string BaseName,
        bool IsAnnounced, InventoryLook? Inventory, Underfoot? Underfoot) : WorldEvent(IsVisible), IInventoryEvent, IUnderfootEvent;
    public record ItemRenamed(EntityRef Entity, InventoryLook? Inventory, Underfoot? Underfoot)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IInventoryEvent, IUnderfootEvent;
    public record ItemsReordered(EntityRef Entity, int FirstIndex, int SecondIndex, InventoryLook? Inventory)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IInventoryEvent;
    public record InventoryRefreshedForDebug(InventoryLook? Inventory, Underfoot? Underfoot)
        : AlwaysVisibleEvent, IInventoryEvent, IUnderfootEvent;
    public record MoneyPickedUp(CharacterLabel Label, int Amount, int Money, ObtainedItem Obtained) : AlwaysVisibleEvent, IItemObtainedEvent;

    public enum ItemActionFailure
    {
        CursedCannotThrow,
        CursedCannotDiscard,
        CursedCannotUse,
        CursedCannotUnequip,
        Illiterate,
        CannotGive,
    }

    public enum FacilityFailure
    {
        CursedCannotPutIn,
        CannotTakeOut,
        CannotPickUp,
    }

    public record ItemActionFailed(bool IsVisible, ItemName ItemName, ItemActionFailure Failure) : WorldEvent(IsVisible);
    public record FacilityItemFailed(ItemName ItemName, FacilityFailure Failure) : AlwaysVisibleEvent;

    public enum ItemChangeKind
    {
        Cursed,
        Uncursed,
        CurseRevealed,
        BecameUndiscardable,
        Repaired,
        NotConsumed,
        Upgraded,
        Downgraded,
        Burned,
        Consumed,
        Used,
        Equipped,
        Unequipped,
        CurseIdentified,
        UsedWhileCursed,
        Recharged,
        Transformed,
        Duplicated,
    }

    public record ItemChanged(bool IsVisible, ItemName ItemName, ItemChangeKind Kind, ItemLook Item, InventoryLook? Inventory,
        Underfoot? Underfoot, ShopLook? Shop) : WorldEvent(IsVisible), IInventoryEvent, IUnderfootEvent, IShopEvent;
    public record ItemChangeResisted(bool IsVisible, ItemName ItemName, ItemChangeKind Kind) : WorldEvent(IsVisible);
}
