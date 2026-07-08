#nullable enable
using Game;
using R3;
using VContainer;
using View.Playback;

namespace Provider.Presentations
{
    internal sealed class WorldEventTranslator
    {
        [Inject]
        public WorldEventTranslator(IReadOnlyWorld world, PlaybackQueue queue, WorldEventPresentations presentations)
        {
            world.Events.OnRecorded.Subscribe(worldEvent =>
            {
                if (presentations.Find(worldEvent) is { } presentation)
                    queue.Enqueue(presentation.Present(worldEvent));
            });
        }
    }
}
