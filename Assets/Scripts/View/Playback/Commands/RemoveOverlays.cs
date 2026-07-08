#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace View.Playback.Commands
{
    public sealed record RemoveOverlays(IReadOnlyList<Vector2Int> Positions) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Tiles.RemoveOverlays(Positions);
        }
    }
}
