#nullable enable

namespace View.Playback
{
    public abstract record ViewOp
    {
        internal abstract float Apply(PlaybackContext context);

        internal virtual bool IsWalkOf(EntityKey key) => false;
    }

    public abstract record InstantOp : ViewOp
    {
        internal sealed override float Apply(PlaybackContext context)
        {
            Run(context);
            return 0;
        }

        private protected abstract void Run(PlaybackContext context);
    }
}
