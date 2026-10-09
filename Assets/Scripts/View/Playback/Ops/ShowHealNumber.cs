#nullable enable
using Configuration;
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record ShowHealNumber(Vector2Int Position, int Amount, int PercentOfMaxHp) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.DamageTexts.ShowHeal(Position, Amount, PercentOfMaxHp,
                Settings.GlobalSettings.DamageTextDisplayTime.CurrentValue);
        }
    }
}
