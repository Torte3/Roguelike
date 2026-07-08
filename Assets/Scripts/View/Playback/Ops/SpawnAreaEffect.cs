#nullable enable
using Configuration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record SpawnAreaEffect(IReadOnlyList<Vector2Int> Area, Color Color) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Effects.Spawn(Area.Where(context.Sight.Contains), Color,
                Settings.GlobalSettings.EffectDisplayTime.CurrentValue);
        }
    }
}
