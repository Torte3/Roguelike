#nullable enable
using UnityEngine;

namespace View.Playback.Commands
{
    public sealed record ShowHealNumber(Vector2Int Position, int Amount, float FontSize) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.DamageTexts.ShowHeal(Position, Amount, FontSize, context.Settings.DamageTextDisplayTime.CurrentValue);
        }
    }
}
