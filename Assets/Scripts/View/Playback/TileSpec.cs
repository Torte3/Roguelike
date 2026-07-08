#nullable enable
using UnityEngine;

namespace View.Playback
{
    public sealed record TileSpec(Vector2Int Position, TileSet Set, TileKind Kind, bool IsKnown);
}
