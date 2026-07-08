#nullable enable
using System;
using UnityEngine;

namespace View.Playback.Commands
{
    public sealed record PlaceEntity(Guid Key, Vector2Int Position, bool IsVisible) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.Place(Key, Position, IsVisible);
        }
    }
}
