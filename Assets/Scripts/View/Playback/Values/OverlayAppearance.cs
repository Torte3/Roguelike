#nullable enable
using UnityEngine;
using View.Map;

namespace View.Playback.Values
{
    public sealed record OverlayAppearance(Vector2Int Position, OverlayKind Kind, TileSet Set, TileVisibility Visibility);
}
