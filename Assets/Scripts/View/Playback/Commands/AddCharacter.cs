#nullable enable
using System;
using UnityEngine;
using View.Playback.Values;

namespace View.Playback.Commands
{
    public sealed record AddCharacter(Guid Key, Vector2Int Position, bool IsShiny, bool IsVisible, CharacterAppearance Appearance) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.AddCharacter(Key, Position, IsShiny, IsVisible, Appearance);
        }
    }
}
