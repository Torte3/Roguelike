#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Map
{
    internal sealed class MapEnteredPresentation : Presentation<MapEntered>
    {
        protected override IEnumerable<ViewOp> OpsOf(MapEntered worldEvent)
        {
            yield return new RebuildView(worldEvent.VisibleArea);
            yield return new SetDungeonInfo(worldEvent.Name, worldEvent.Depth);
            yield return new EnterTiles(
                Tiles.SetOf(worldEvent.Type),
                worldEvent.ShopRect,
                worldEvent.Tiles.Select(Tiles.Of).ToList(),
                worldEvent.OverlayTiles.Select(tile => Tiles.Of(tile.Position, tile.Category)).ToList());
            yield return new PlaceCamera(worldEvent.PlayerPosition, worldEvent.Bounds);
            yield return new PlayBgm(TrackOf(worldEvent));
            yield return new SetStatusMoney(worldEvent.Money);
            if (worldEvent.Shop == null)
                yield return new HideShopPrices();
            foreach (var op in ShopPrices.Of(worldEvent.Shop))
                yield return op;
            foreach (var op in InventoryRows.Rebuilt(worldEvent.Inventory, worldEvent.IsNewWorld))
                yield return op;
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
        }

        private static BgmTrack TrackOf(MapEntered worldEvent)
        {
            if (worldEvent.IsShopStolen)
                return BgmTrack.Stolen;
            return worldEvent.IsInShop ? BgmTrack.Shop : BgmTrack.Normal;
        }
    }
}
