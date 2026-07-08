#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;
using View.Playback;

namespace Provider.Presentations.Combat.SkillSources
{
    internal abstract class SkillSourcePresentation<TSource> : ISkillSourcePresentation where TSource : SkillSource
    {
        public bool IsWaitingForMovers(SkillUsed used) => IsWaitingForMovers(used, (TSource)used.Source);

        public IEnumerable<ViewCommand> CommandsOf(SkillUsed used) => CommandsOf(used, (TSource)used.Source);

        public IEnumerable<ViewCommand> VisibleCommandsOf(SkillUsed used) => VisibleCommandsOf(used, (TSource)used.Source);

        protected abstract bool IsWaitingForMovers(SkillUsed used, TSource source);

        protected virtual IEnumerable<ViewCommand> CommandsOf(SkillUsed used, TSource source) =>
            Enumerable.Empty<ViewCommand>();

        protected virtual IEnumerable<ViewCommand> VisibleCommandsOf(SkillUsed used, TSource source) =>
            Enumerable.Empty<ViewCommand>();

        public abstract ViewCommand ResultLine(string text);
    }
}
