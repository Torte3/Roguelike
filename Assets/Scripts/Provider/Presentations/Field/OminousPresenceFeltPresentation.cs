#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Field
{
    internal sealed class OminousPresenceFeltPresentation : Presentation<OminousPresenceFelt>
    {
        protected override IEnumerable<ViewOp> OpsOf(OminousPresenceFelt worldEvent)
        {
            yield return Logs.Line("不穏な気配を感じる……".Paint(Tint.Notice));
        }
    }
}
