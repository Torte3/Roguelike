#nullable enable
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record WalkEntity(EntityKey Key, Vector2Int Position, bool IsVisibleAfter) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            var seconds = context.Queue.TakeWalkSeconds(Key);
            context.Entities.Walk(Key, Position, IsVisibleAfter, seconds);
            context.Queue.MarkBusy(Key, seconds);
        }

        internal override bool IsWalkOf(EntityKey key) => key == Key;
    }
}
