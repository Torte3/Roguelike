#nullable enable
using System.Collections.Generic;
using View.Playback.Values;

namespace View.Playback.Commands
{
    public sealed record ChangeOverlays(IReadOnlyList<OverlayAppearance> Overlays) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Tiles.ChangeOverlays(Overlays);
        }
    }
}
