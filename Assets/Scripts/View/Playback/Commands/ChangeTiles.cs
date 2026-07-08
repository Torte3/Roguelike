#nullable enable
using System.Collections.Generic;
using View.Playback.Values;

namespace View.Playback.Commands
{
    public sealed record ChangeTiles(IReadOnlyList<TileAppearance> Tiles, IReadOnlyList<PositionedTileVisibility> Visibilities) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Tiles.Change(Tiles, Visibilities);
        }
    }
}
