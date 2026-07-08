#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;
using View.Playback;

namespace Provider.Presentations
{
    internal abstract class Presentation<TEvent> : IWorldEventPresentation where TEvent : WorldEvent
    {
        public Type EventType => typeof(TEvent);

        public PlaybackStep Present(WorldEvent worldEvent)
        {
            var typed = (TEvent)worldEvent;
            return new PlaybackStep(SubjectOf(typed), WaitsForMovers(typed), OpsOf(typed).ToList());
        }

        protected virtual EntityKey? SubjectOf(TEvent worldEvent) => null;

        protected virtual bool WaitsForMovers(TEvent worldEvent) => false;

        protected abstract IEnumerable<ViewOp> OpsOf(TEvent worldEvent);
    }
}
