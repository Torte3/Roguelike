#nullable enable
using UnityEngine;
using View.Map;

namespace View.Playback.Values
{
    public sealed record TileAppearance(Vector2Int Position, TileSet Set, int PaletteIndex, MinimapPaint Minimap);
}
