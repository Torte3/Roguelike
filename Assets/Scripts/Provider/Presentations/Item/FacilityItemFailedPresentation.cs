#nullable enable
using Domain.Model.WorldEvents;
using Provider.Texts;
using System;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Item
{
    internal sealed class FacilityItemFailedPresentation : Presentation<FacilityItemFailed>
    {
        protected override IEnumerable<ViewCommand> CommandsOf(FacilityItemFailed worldEvent)
        {
            yield return AddLog.Line(TextOf(worldEvent));
        }

        private string TextOf(FacilityItemFailed failed)
        {
            var itemName = new ItemNameText(failed.ItemName).Text;
            return (failed.Action, failed.Failure) switch
            {
                (FacilityAction.PutIn, FacilityFailure.Cursed) => $"{itemName}は呪われていて入れられない",
                (FacilityAction.TakeOut, FacilityFailure.Unavailable) => $"{itemName}は取り出せなかった",
                (FacilityAction.PickUp, FacilityFailure.Unavailable) => $"{itemName}を拾えなかった",
                _ => throw new ArgumentOutOfRangeException(nameof(failed), (failed.Action, failed.Failure), null),
            };
        }
    }
}
