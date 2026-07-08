#nullable enable

namespace View.Playback.Ops
{
    public sealed record SetAffiliationMarker(EntityKey Key, bool IsEnemy, bool IsAlly) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetAffiliation(Key, IsEnemy, IsAlly);
        }
    }
}
