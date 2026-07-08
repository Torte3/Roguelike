#nullable enable
using UnityEngine;
using View.Effects;

namespace View.Playback.Commands
{
    public sealed record PopupIcon(Sprite Icon, Vector2 Position) : InstantCommand
    {
        private static readonly Vector2 Offset = new(0f, 0.9f);

        private protected override void Run(PlaybackContext context)
        {
            WorldIconPopup.Show(Icon, Position + Offset);
        }
    }
}
