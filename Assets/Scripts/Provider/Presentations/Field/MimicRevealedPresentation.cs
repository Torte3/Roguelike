#nullable enable
using Domain.Model.WorldEvents;
using Provider.Texts.Labels;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Field
{
    internal sealed class MimicRevealedPresentation : Presentation<MimicRevealed>
    {
        protected override IEnumerable<ViewCommand> VisibleCommandsOf(MimicRevealed worldEvent)
        {
            yield return AddLog.Line($"{EntityLabelText.Of(worldEvent.Disguise).Text}は{worldEvent.MimicName}の擬態だった！");
        }
    }
}
