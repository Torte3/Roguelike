#nullable enable
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record PlaceCamera(Vector2Int Position, RectInt Bounds) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Camera.SetPosition((Vector3Int)Position);
            context.CameraRect.SetRect(Bounds);
        }
    }
}
