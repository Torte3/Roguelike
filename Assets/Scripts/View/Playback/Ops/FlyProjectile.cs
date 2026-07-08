#nullable enable
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record FlyProjectile(Sprite Icon, Vector2Int From, Vector2Int To) : ViewOp
    {
        internal override float Apply(PlaybackContext context)
        {
            return context.Projectiles.Fly(Icon, From, To);
        }
    }
}
