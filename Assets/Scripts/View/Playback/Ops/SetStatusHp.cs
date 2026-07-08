#nullable enable
using Configuration;
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record SetStatusHp(int Hp, int MaxHp) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Status.SetHp(MaxHp, Hp);
            var isLow = Hp * 100 / MaxHp < Settings.GlobalSettings.LowHpThresholdPercentage.CurrentValue;
            context.Status.SetTextColor(isLow ? Color.red : Color.white);
        }
    }
}
