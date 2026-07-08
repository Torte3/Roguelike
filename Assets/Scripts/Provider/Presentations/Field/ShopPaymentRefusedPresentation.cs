#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Field
{
    internal sealed class ShopPaymentRefusedPresentation : Presentation<ShopPaymentRefused>
    {
        protected override IEnumerable<ViewCommand> CommandsOf(ShopPaymentRefused worldEvent)
        {
            yield return AddLog.Line($"{new CharacterNameText(worldEvent.Label).Text}は{Tint.Notice.Paint($"{worldEvent.Shortfall}G")}持っていなかった");
        }
    }
}
