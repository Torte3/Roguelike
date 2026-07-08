#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record ChangeSight(IReadOnlyCollection<Vector2Int> Area) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Sight.Change(Area);
        }
    }
}
