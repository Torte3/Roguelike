#nullable enable

namespace View.Playback.Ops
{
    public sealed record SetHpBar(EntityKey Key, int Hp, int MaxHp) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetHp(Key, Hp, MaxHp);
        }
    }
}
