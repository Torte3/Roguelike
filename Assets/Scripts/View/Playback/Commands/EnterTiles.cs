#nullable enable
using System.Collections.Generic;
using View.Playback.Values;

namespace View.Playback.Commands
{
    public sealed record EnterTiles(IReadOnlyList<TileAppearance> Tiles, IReadOnlyList<OverlayAppearance> Overlays,
        IReadOnlyList<PositionedTileVisibility> Visibilities) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Tiles.Enter(Tiles, Overlays, Visibilities);
        }
    }
}
