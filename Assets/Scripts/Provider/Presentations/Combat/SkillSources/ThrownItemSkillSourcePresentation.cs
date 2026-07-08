#nullable enable
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Combat.SkillSources
{
    internal sealed class ThrownItemSkillSourcePresentation : SkillSourcePresentation<ThrownItemSkillSource>
    {
        protected override bool IsWaitingForMovers(SkillUsed used, ThrownItemSkillSource source) => false;

        public override ViewCommand ResultLine(string text) => AddLog.Appended(text);
    }
}
