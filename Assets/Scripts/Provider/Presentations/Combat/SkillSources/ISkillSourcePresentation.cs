#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;

namespace Provider.Presentations.Combat.SkillSources
{
    internal interface ISkillSourcePresentation
    {
        public bool IsWaitingForMovers(SkillUsed used);
        public IEnumerable<ViewCommand> CommandsOf(SkillUsed used);
        public IEnumerable<ViewCommand> VisibleCommandsOf(SkillUsed used);
        public ViewCommand ResultLine(string text);
    }
}
