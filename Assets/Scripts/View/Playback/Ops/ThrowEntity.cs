#nullable enable
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record ThrowEntity(EntityKey Key, Vector2Int Position, bool IsVisibleAfter) : ViewOp
    {
        internal override float Apply(PlaybackContext context)
        {
            var from = context.Entities.PositionOf(Key) ?? Position;
            var seconds = context.Projectiles.SecondsBetween(from, Position);
            context.Entities.Glide(Key, Position, IsVisibleAfter, seconds);
            return seconds;
        }
    }
}
