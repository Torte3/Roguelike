#nullable enable
using Domain.Model.WorldEvents;
using Provider.Texts;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;
using View.Playback.Values;

namespace Provider.Presentations.Item
{
    internal sealed class MoneyPickedUpPresentation : Presentation<MoneyPickedUp>
    {
        protected override IEnumerable<ViewCommand> CommandsOf(MoneyPickedUp worldEvent)
        {
            yield return AddLog.Line($"{new CharacterNameText(worldEvent.Label).Text}は{worldEvent.Amount}Gを拾った");
            yield return new PlaySE(SEKind.MoneyPickup);
            yield return new SetStatusMoney(worldEvent.Money);
            yield return new PopupIcon(worldEvent.Obtained.Icon, worldEvent.Obtained.Position);
        }
    }
}
