#nullable enable

namespace View.Playback.Ops
{
    public sealed record SetChargeTurns(EntityKey Key, int Turns) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetChargeTurns(Key, Turns);
        }
    }
}
