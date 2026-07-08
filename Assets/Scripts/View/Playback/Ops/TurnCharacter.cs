#nullable enable
using Utilities;

namespace View.Playback.Ops
{
    public sealed record TurnCharacter(EntityKey Key, Direction8 Direction) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.Turn(Key, Direction);
        }
    }
}
