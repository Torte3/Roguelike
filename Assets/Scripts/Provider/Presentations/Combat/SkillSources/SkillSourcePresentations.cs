#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;

namespace Provider.Presentations.Combat.SkillSources
{
    internal static class SkillSourcePresentations
    {
        private static readonly IReadOnlyDictionary<Type, ISkillSourcePresentation> s_presentations =
            new ISkillSourcePresentation[]
            {
                new CharacterSkillSourcePresentation(),
                new ItemSkillSourcePresentation(),
                new DeviceSkillSourcePresentation(),
            }.ToDictionary(presentation => presentation.SourceType);

        public static ISkillSourcePresentation Of(SkillSource source) => s_presentations[source.GetType()];
    }
}
