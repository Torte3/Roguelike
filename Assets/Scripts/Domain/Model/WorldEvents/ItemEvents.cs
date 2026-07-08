#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Item.Description;
using UnityEngine;
using Utilities;

namespace Domain.Model.WorldEvents
{
    public record ObtainedItem(Sprite Icon, Vector2Int Position);

    public record ItemBroken(bool IsVisible, ItemName ItemName) : WorldEvent(IsVisible);
    public record ItemThrown(EntityRef Entity, CharacterLabel Label, ItemName ItemName, Flight Flight)
        : WorldEvent(Entity.IsVisible);
    public record ItemDropped(EntityRef Entity, CharacterLabel Label, ItemName ItemName) : WorldEvent(Entity.IsVisible);
    public record ItemDiscarded(EntityRef Entity, CharacterLabel Label, ItemName ItemName) : WorldEvent(Entity.IsVisible);
    public record ItemExchanged(EntityRef Entity, CharacterLabel Label, ItemName DroppedName, ItemName PickedUpName,
        ObtainedItem Obtained)
        : WorldEvent(Entity.IsVisible);
    public record ItemPickedUp(EntityRef Entity, CharacterLabel Label, ItemName ItemName, bool IsAutomatic, ObtainedItem Obtained)
        : WorldEvent(Entity.IsVisible);
    public record ItemSteppedOn(EntityRef Entity, CharacterLabel Label, ItemName ItemName) : WorldEvent(Entity.IsVisible);
    public record ItemGiven(EntityRef Entity, CharacterLabel Label, ItemName ItemName) : WorldEvent(Entity.IsVisible);
    public record ItemObtainedFromChest(CharacterLabel Label, ItemName ItemName, ObtainedItem Obtained)
        : AlwaysVisibleEvent;
    public record ItemsMerged(CharacterLabel Label, ItemName BaseItemName, ItemName MergedItemName) : AlwaysVisibleEvent;
    public record MergedItemNotStored : AlwaysVisibleEvent;
    public record ItemIdentified(bool IsVisible, ItemName NameBefore, string BaseName, DiscoveryCause Cause)
        : WorldEvent(IsVisible);
    public record ItemCurseIdentified(bool IsVisible, ItemName ItemName, bool IsCursed, DiscoveryCause Cause)
        : WorldEvent(IsVisible);
    public record ItemRenamed(EntityRef Entity) : WorldEvent(Entity.IsVisible);
    public record ItemsReordered(EntityRef Entity, int FirstIndex, int SecondIndex) : WorldEvent(Entity.IsVisible);
    public record PlayerUnderfootChanged(Underfoot Underfoot) : AlwaysVisibleEvent;
    public record InventoryRowsChanged(EntityRef Entity, IReadOnlyList<InventoryRowChange> Changes) : AlwaysVisibleEvent;
    public record ItemUpdated(ItemLook Item) : AlwaysVisibleEvent;
    public record MoneyPickedUp(CharacterLabel Label, int Amount, int Money, ObtainedItem Obtained) : AlwaysVisibleEvent;

    public enum ItemAction
    {
        Use,
        Throw,
        Discard,
        Give,
    }

    public enum ItemActionFailure
    {
        Cursed,
        Illiterate,
        Unavailable,
    }

    public enum FacilityAction
    {
        PutIn,
        TakeOut,
        PickUp,
    }

    public enum FacilityFailure
    {
        Cursed,
        Unavailable,
    }

    public record ItemActionFailed(bool IsVisible, ItemName ItemName, EquipState EquipState, ItemAction Action,
        ItemActionFailure Failure) : WorldEvent(IsVisible);
    public record FacilityItemFailed(ItemName ItemName, FacilityAction Action, FacilityFailure Failure) : AlwaysVisibleEvent;

    public enum ItemChangeKind
    {
        Cursed,
        Uncursed,
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
        UsedWhileCursed,
        Recharged,
        Transformed,
        Duplicated,
    }

    public record ItemChanged(bool IsVisible, ItemName NameBefore, ItemChangeKind Kind, ItemLook Item) : WorldEvent(IsVisible);
    public record ItemChangeResisted(bool IsVisible, ItemName ItemName, ItemChangeKind Kind) : WorldEvent(IsVisible);
}
