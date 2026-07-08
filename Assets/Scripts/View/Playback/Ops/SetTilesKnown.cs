#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record SetTilesKnown(IReadOnlyList<Vector2Int> Positions, bool IsKnown) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Tiles.SetKnown(Positions, IsKnown);
        }
    }
}
