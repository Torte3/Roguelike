#nullable enable
using Configuration;

namespace View.Playback.Ops
{
    public sealed record FlashOnPlayerDamage(int DamagePercentOfMaxHp, int HpPercentOfMaxHp) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            var settings = Settings.GlobalSettings;
            if (DamagePercentOfMaxHp > settings.SignificantDamageThresholdPercentage.CurrentValue ||
                HpPercentOfMaxHp < settings.LowHpThresholdPercentage.CurrentValue)
                context.Flush.Flush(settings.FlushDuration.CurrentValue);
        }
    }
}
