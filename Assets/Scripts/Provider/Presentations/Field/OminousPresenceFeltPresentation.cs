#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Field
{
    internal sealed class OminousPresenceFeltPresentation : Presentation<OminousPresenceFelt>
    {
        protected override IEnumerable<ViewCommand> CommandsOf(OminousPresenceFelt worldEvent)
        {
            yield return AddLog.Line(Tint.Notice.Paint("不穏な気配を感じる……"));
        }
    }
}
