#nullable enable
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record PopupIcon(Sprite Icon, Vector2 Position) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            WorldIconPopup.Show(Icon, Position);
        }
    }
}
