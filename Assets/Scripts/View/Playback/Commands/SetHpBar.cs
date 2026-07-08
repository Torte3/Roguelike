#nullable enable
using System;

namespace View.Playback.Commands
{
    public sealed record SetHpBar(Guid Key, int Hp, int MaxHp) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetHp(Key, Hp, MaxHp);
        }
    }
}
