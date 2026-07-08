#nullable enable
namespace View.Playback
{
    public abstract record InstantCommand : ViewCommand
    {
        internal sealed override float ApplyAndGetBlockSeconds(PlaybackContext context)
        {
            Run(context);
            return 0;
        }

        private protected abstract void Run(PlaybackContext context);
    }
}
