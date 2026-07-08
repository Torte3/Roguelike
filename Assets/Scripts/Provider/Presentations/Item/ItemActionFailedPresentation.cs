#nullable enable
using Domain.Model.Item;
using Domain.Model.WorldEvents;
using Provider.Texts;
using System;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Item
{
    internal sealed class ItemActionFailedPresentation : Presentation<ItemActionFailed>
    {
        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ItemActionFailed worldEvent)
        {
            yield return AddLog.Line(TextOf(worldEvent));
        }

        private string TextOf(ItemActionFailed failed)
        {
            var itemName = new ItemNameText(failed.ItemName).Text;
            return (failed.Action, failed.Failure) switch
            {
                (_, ItemActionFailure.Illiterate) => $"{itemName}は文字が読めない",
                (ItemAction.Throw, ItemActionFailure.Cursed) => $"{itemName}は呪われていて投げられない",
                (ItemAction.Discard, ItemActionFailure.Cursed) => $"{itemName}は呪われていて捨てられない",
                (ItemAction.Use, ItemActionFailure.Cursed) => failed.EquipState == EquipState.Equipped
                    ? $"{itemName}は呪われていて外せない"
                    : $"{itemName}は呪われているため使用できない",
                (ItemAction.Give, ItemActionFailure.Unavailable) => $"{itemName}を渡せなかった。",
                _ => throw new ArgumentOutOfRangeException(nameof(failed), (failed.Action, failed.Failure), null),
            };
        }
    }
}
