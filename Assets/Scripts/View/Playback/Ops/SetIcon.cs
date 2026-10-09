#nullable enable
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record SetIcon(EntityKey Key, Sprite? Icon) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetIcon(Key, Icon);
        }
    }
}
