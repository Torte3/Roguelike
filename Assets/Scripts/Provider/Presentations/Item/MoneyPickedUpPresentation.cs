#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class MoneyPickedUpPresentation : Presentation<MoneyPickedUp>
    {
        protected override IEnumerable<ViewOp> OpsOf(MoneyPickedUp worldEvent)
        {
            yield return Logs.Line($"{Names.Of(worldEvent.Label)}は{worldEvent.Amount}Gを拾った");
            yield return new PlaySe(SeKind.MoneyPickup);
            yield return new SetStatusMoney(worldEvent.Money);
            yield return Obtained.Popup(worldEvent.Obtained);
        }
    }
}
