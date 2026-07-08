#nullable enable
using Domain.Model.Character;
using Domain.Model.Item;
using Utilities;

namespace Domain.Model.WorldEvents
{
    public record MimicRevealed(bool IsVisible, EntityLabel Disguise, string MimicName) : WorldEvent(IsVisible);
    public record DeviceBroken(bool IsVisible, string DeviceName) : WorldEvent(IsVisible);
    public record MonsterHouseEntered : AlwaysVisibleEvent;
    public record OminousPresenceFelt : AlwaysVisibleEvent;
    public record TheftDetected : AlwaysVisibleEvent;
    public record ShopEntered : AlwaysVisibleEvent;
    public record ShopExited : AlwaysVisibleEvent;

    public record ShopSettled(CharacterLabel Label, int Received, int Paid, int Money) : AlwaysVisibleEvent;
    public record ShopTotalsChanged(int PurchasePrice, int SalePrice) : AlwaysVisibleEvent;
    public record ItemPriceChanged(Id<IItem> Item, int Price, bool IsShopItem) : AlwaysVisibleEvent;
    public record ShopPaymentRefused(CharacterLabel Label, int Shortfall) : AlwaysVisibleEvent;

    public record FacilityUsed(FixtureKind Kind) : AlwaysVisibleEvent;
}
