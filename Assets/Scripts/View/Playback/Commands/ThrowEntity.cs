#nullable enable
using System;
using UnityEngine;

namespace View.Playback.Commands
{
    public sealed record ThrowEntity(Guid Key, Vector2Int Position, bool IsVisibleAfter) : ViewCommand
    {
        internal override float ApplyAndGetBlockSeconds(PlaybackContext context)
        {
            var from = context.Entities.PositionOf(Key) ?? Position;
            var seconds = context.Projectiles.SecondsBetween(from, Position);
            context.Entities.Glide(Key, Position, IsVisibleAfter, seconds);
            return seconds;
        }
    }
}
