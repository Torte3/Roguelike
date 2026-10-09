#nullable enable
using UnityEngine;

namespace View.Playback
{
    public sealed record OverlaySpec(Vector2Int Position, OverlayKind? Kind);
}
