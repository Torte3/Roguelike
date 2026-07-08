#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record RebuildView(IReadOnlyCollection<Vector2Int> VisibleArea) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Queue.ResetTimings();
            context.Sight.Reset(VisibleArea);
            context.Entities.Clear();
            context.DamageTexts.DeleteAllText();
        }
    }
}
