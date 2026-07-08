#nullable enable
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record PlaceEntity(EntityKey Key, Vector2Int Position, bool IsVisible) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.Place(Key, Position, IsVisible);
        }
    }
}
