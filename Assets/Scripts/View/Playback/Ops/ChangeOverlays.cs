#nullable enable
using System.Collections.Generic;

namespace View.Playback.Ops
{
    public sealed record ChangeOverlays(IReadOnlyList<OverlaySpec> Overlays) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Tiles.ChangeOverlays(Overlays);
        }
    }
}
