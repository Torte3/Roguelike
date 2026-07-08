#nullable enable
using Configuration;
using System.Collections.Generic;
using UnityEngine;

namespace View.Playback.Commands
{
    public sealed record SpawnAreaEffect(IReadOnlyList<Vector2Int> Area, Color Color) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Effects.Spawn(Area, Color,
                Settings.GlobalSettings.EffectDisplayTime.CurrentValue);
        }
    }
}
