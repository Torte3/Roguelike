#nullable enable
using Domain.Model.WorldEvents;
using View.Playback.Values;

namespace Provider.Presentations.Tile
{
    internal sealed record TileKind(int PaletteIndex, MinimapPaint Minimap)
    {
        public static TileKind Floor { get; } = new(0, MinimapPaint.Floor);
        public static TileKind ShopFloor { get; } = new(1, MinimapPaint.Floor);
        public static TileKind Water { get; } = new(2, MinimapPaint.Water);
        public static TileKind Wall { get; } = new(3, MinimapPaint.Wall);
        public static TileKind UnbreakableWall { get; } = new(3, MinimapPaint.UnbreakableWall);

        public TileAppearance AppearanceAt(TileState tile) => new(tile.Position, new MapTiles(tile.MapType).Set, PaletteIndex, Minimap);
    }
}
