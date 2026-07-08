#nullable enable
using Domain.Model.WorldEvents;
using Provider.Texts;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Combat.SkillSources
{
    internal sealed class CharacterSkillSourcePresentation : SkillSourcePresentation<CharacterSkillSource>
    {
        protected override bool IsWaitingForMovers(SkillUsed used, CharacterSkillSource source) => used.IsVisible;

        protected override IEnumerable<ViewCommand> CommandsOf(SkillUsed used, CharacterSkillSource source)
        {
            yield return new PlayAttack(used.Entity.Id.Value);
        }

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(SkillUsed used, CharacterSkillSource source)
        {
            if (source.Log != "")
                yield return AddLog.Line($"{new CharacterNameText(source.Label).Text}{source.Log}");
        }

        public override ViewCommand ResultLine(string text) => AddLog.Appended(text);
    }
}
