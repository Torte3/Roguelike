#nullable enable
using System;
using UnityEngine;
using View.Playback.Figures;

namespace View.Playback.Commands
{
    public sealed record AddEntity(Guid Key, FigureBlueprint Figure, Vector2Int Position, Sprite? Icon, bool IsShiny, bool IsVisible) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.Add(Key, Figure, Position, Icon, IsShiny, IsVisible);
        }
    }
}
