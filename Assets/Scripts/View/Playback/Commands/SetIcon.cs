#nullable enable
using System;
using UnityEngine;

namespace View.Playback.Commands
{
    public sealed record SetIcon(Guid Key, Sprite? Icon) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetIcon(Key, Icon);
        }
    }
}
