#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Commands;
using View.Playback.Values;

namespace Provider.Presentations.Field
{
    internal sealed class ShopSettledPresentation : Presentation<ShopSettled>
    {
        protected override IEnumerable<ViewCommand> CommandsOf(ShopSettled worldEvent)
        {
            var name = new CharacterNameText(worldEvent.Label).Text;
            if (worldEvent.Received > 0)
                yield return AddLog.Line($"{name}は{Tint.Gain.Paint($"{worldEvent.Received}G")}受け取った");
            if (worldEvent.Paid > 0)
                yield return AddLog.Line($"{name}は{Tint.Notice.Paint($"{worldEvent.Paid}G")}支払った");
            yield return new PlaySE(SEKind.ShopCheckout);
            yield return new SetStatusMoney(worldEvent.Money);
        }
    }
}
