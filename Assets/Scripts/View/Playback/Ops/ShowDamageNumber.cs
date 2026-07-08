#nullable enable
using Configuration;
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record ShowDamageNumber(Vector2Int Position, int Amount, int PercentOfMaxHp) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.DamageTexts.ShowDamage(Position, Amount, PercentOfMaxHp,
                Settings.GlobalSettings.DamageTextDisplayTime.CurrentValue);
        }
    }
}
