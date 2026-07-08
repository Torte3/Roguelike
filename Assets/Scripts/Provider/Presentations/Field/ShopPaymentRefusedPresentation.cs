#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Field
{
    internal sealed class ShopPaymentRefusedPresentation : Presentation<ShopPaymentRefused>
    {
        protected override IEnumerable<ViewOp> OpsOf(ShopPaymentRefused worldEvent)
        {
            yield return Logs.Line($"{Names.Of(worldEvent.Label)}は{$"{worldEvent.Shortfall}G".Paint(Tint.Notice)}持っていなかった");
        }
    }
}
