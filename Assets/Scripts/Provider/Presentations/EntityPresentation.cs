#nullable enable
using Domain.Model.WorldEvents;
using View.Playback;

namespace Provider.Presentations
{
    internal abstract class EntityPresentation<TEvent> : Presentation<TEvent>
        where TEvent : WorldEvent, IEntityWorldEvent
    {
        protected sealed override EntityKey? SubjectOf(TEvent worldEvent) => worldEvent.Entity.Key();
    }
}
