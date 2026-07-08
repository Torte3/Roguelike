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
            var commands = CommandsOf(typed)
                .Concat(typed.IsVisible ? VisibleCommandsOf(typed) : HiddenCommandsOf(typed));
            return new PlaybackStep(SubjectOf(typed), IsWaitingForMovers(typed), commands.ToList());
        }

        protected static Guid KeyOf(EntityRef entity) => entity.Id.Value;

        protected virtual Guid? SubjectOf(TEvent worldEvent) => null;

        protected virtual bool IsWaitingForMovers(TEvent worldEvent) => false;

        protected virtual IEnumerable<ViewCommand> CommandsOf(TEvent worldEvent) => Enumerable.Empty<ViewCommand>();

        protected virtual IEnumerable<ViewCommand> VisibleCommandsOf(TEvent worldEvent) => Enumerable.Empty<ViewCommand>();

        protected virtual IEnumerable<ViewCommand> HiddenCommandsOf(TEvent worldEvent) => Enumerable.Empty<ViewCommand>();
    }
}
