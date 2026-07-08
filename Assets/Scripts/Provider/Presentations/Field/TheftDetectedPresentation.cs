#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Inventory;
using Provider.Presentations.Translation;
using Provider.Texts;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Field
{
    internal sealed class TheftDetectedPresentation : ShopRelationPresentation<TheftDetected>
    {
        public TheftDetectedPresentation(BGMSelector bgm, ShopPrices shopPrices, InventoryRows inventoryRows)
            : base(bgm, shopPrices, inventoryRows, new ShopAfterTheft())
        {
        }

        protected override bool IsWaitingForMovers(TheftDetected worldEvent) => true;

        protected override IEnumerable<ViewCommand> CommandsOf(TheftDetected worldEvent)
        {
            foreach (var command in RelationChanged())
                yield return command;
            yield return new Pause(RoomEvents.PauseSeconds);
            yield return AddLog.Line(Tint.Alert.Paint("どろぼう！"));
            yield return Music();
        }
    }
}
