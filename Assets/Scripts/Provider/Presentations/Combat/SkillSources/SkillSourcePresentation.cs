#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;

namespace Provider.Presentations.Combat.SkillSources
{
    internal abstract class SkillSourcePresentation<TSource> : ISkillSourcePresentation where TSource : SkillSource
    {
        public Type SourceType => typeof(TSource);

        public bool WaitsForMovers(SkillUsed used) => WaitsForMovers(used, (TSource)used.Source);

        public IEnumerable<ViewOp> OpsOf(SkillUsed used) => OpsOf(used, (TSource)used.Source);

        protected abstract bool WaitsForMovers(SkillUsed used, TSource source);

        protected abstract IEnumerable<ViewOp> OpsOf(SkillUsed used, TSource source);
    }
}
