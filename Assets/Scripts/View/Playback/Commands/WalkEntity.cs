#nullable enable
using System;
using UnityEngine;

namespace View.Playback.Commands
{
    public sealed record WalkEntity(Guid Key, Vector2Int Position, bool IsVisibleAfter) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            var seconds = context.Clock.TakeWalkSeconds(Key);
            context.Entities.Walk(Key, Position, IsVisibleAfter, seconds);
            context.Clock.MarkBusy(Key, seconds);
        }

        internal override bool IsWalkOf(Guid key) => key == Key;
    }
}
