#nullable enable
using Domain.Model.WorldEvents;
using Game;
using R3;
using VContainer;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations
{
    public sealed class WorldEventTranslator
    {
        [Inject]
        public WorldEventTranslator(IReadOnlyWorld world, PlaybackQueue queue)
        {
            world.Events.OnRecorded.Subscribe(worldEvent =>
            {
                if (WorldEventPresentations.Find(worldEvent) is { } presentation)
                    queue.Enqueue(presentation.Present(worldEvent));
            });
            world.Events.OnCleared.Subscribe(_ => queue.Enqueue(new PlaybackStep(null, false, new ViewOp[] { new ClearLog() })));
        }
    }
}
