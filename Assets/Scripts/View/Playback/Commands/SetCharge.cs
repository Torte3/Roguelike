#nullable enable
using System;
using View.Playback.Values;

namespace View.Playback.Commands
{
    public sealed record SetCharge(Guid Key, ChargePreview? Charge) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetCharge(Key, Charge);
        }
    }
}
