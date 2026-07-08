#nullable enable
using UnityEngine;

namespace View.Playback.Commands
{
    public sealed record FlyProjectile(Sprite Icon, Vector2Int From, Vector2Int To) : ViewCommand
    {
        internal override float ApplyAndGetBlockSeconds(PlaybackContext context)
        {
            return context.Projectiles.Fly(Icon, From, To);
        }
    }
}
