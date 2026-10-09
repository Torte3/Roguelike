#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;

namespace Provider.Presentations.Combat.SkillSources
{
    internal interface ISkillSourcePresentation
    {
        public Type SourceType { get; }
        public bool WaitsForMovers(SkillUsed used);
        public IEnumerable<ViewOp> OpsOf(SkillUsed used);
    }
}
