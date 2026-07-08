#nullable enable
using UnityEngine;
using View.Map;

namespace View.Playback.Values
{
    public sealed record PositionedTileVisibility(Vector2Int Position, TileVisibility Visibility);
}
