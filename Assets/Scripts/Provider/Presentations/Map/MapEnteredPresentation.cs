#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using Provider.Presentations.Inventory;
using Provider.Presentations.Tile;
using Provider.Presentations.Translation;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Map
{
    internal sealed class MapEnteredPresentation : Presentation<MapEntered>
    {
        private readonly BGMSelector _bgm;
        private readonly TileKindClassifier _tileKinds;
        private readonly PlayerSight _sight;
        private readonly TileVisibilities _tileVisibilities;
        private readonly ChargePreviews _charges;
        private readonly ChestKeyHolders _keyHolders;
        private readonly PlayerEntity _player;
        private readonly InventoryRows _inventoryRows;
        private readonly ShopPrices _shopPrices;

        public MapEnteredPresentation(BGMSelector bgm, TileKindClassifier tileKinds, PlayerSight sight,
            TileVisibilities tileVisibilities, ChargePreviews charges, ChestKeyHolders keyHolders, PlayerEntity player,
            InventoryRows inventoryRows, ShopPrices shopPrices)
        {
            _bgm = bgm;
            _tileKinds = tileKinds;
            _sight = sight;
            _tileVisibilities = tileVisibilities;
            _charges = charges;
            _keyHolders = keyHolders;
            _player = player;
            _inventoryRows = inventoryRows;
            _shopPrices = shopPrices;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(MapEntered worldEvent)
        {
            _player.MapEntered(worldEvent.Player);
            _sight.MapEntered(worldEvent.VisibleArea);
            _charges.MapEntered();
            _keyHolders.MapEntered();
            var visibilities = _tileVisibilities.MapEntered(new MapTiles(worldEvent.Type).Set, worldEvent.Tiles);
            var tileKinds = _tileKinds.MapEntered(worldEvent.ShopRect, worldEvent.Tiles);
            var overlays = _tileVisibilities.OverlaysOf(worldEvent.OverlayTiles);
            var music = _bgm.MapEntered(worldEvent.Map.Value, worldEvent.Shop);
            var prices = _shopPrices.RelationChanged(worldEvent.Shop);
            var inventory = _inventoryRows.Rebuilt(worldEvent.Inventory, worldEvent.BareHand);

            var commands = new List<ViewCommand>
            {
                new RebuildView(),
                new SetDungeonInfo($"{worldEvent.Name} B{worldEvent.Depth}F"),
                new EnterTiles(tileKinds, overlays, visibilities),
                new PlaceCamera(worldEvent.PlayerPosition, worldEvent.Bounds),
            };
            commands.AddRange(music);
            commands.Add(new SetStatusMoney(worldEvent.Money));
            commands.Add(prices);
            commands.Add(inventory);
            if (worldEvent.IsNewWorld)
                commands.Add(new ResetInventoryFocus());
            return commands;
        }
    }
}
