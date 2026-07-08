#nullable enable
using UnityEngine;

namespace View.Playback.Commands
{
    public sealed record SetStatusHp(int Hp, int MaxHp, bool IsLow) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Status.SetHp(MaxHp, Hp);
            context.Status.SetTextColor(IsLow ? Color.red : Color.white);
        }
    }
}
