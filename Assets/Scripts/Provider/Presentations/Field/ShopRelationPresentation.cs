#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Inventory;
using Provider.Presentations.Translation;
using View.Playback;

namespace Provider.Presentations.Field
{
    internal abstract class ShopRelationPresentation<TEvent> : Presentation<TEvent> where TEvent : WorldEvent
    {
        private readonly BGMSelector _bgm;
        private readonly ShopPrices _shopPrices;
        private readonly InventoryRows _inventoryRows;
        private readonly ShopRelation _relation;

        protected ShopRelationPresentation(BGMSelector bgm, ShopPrices shopPrices, InventoryRows inventoryRows,
            ShopRelation relation)
        {
            _bgm = bgm;
            _shopPrices = shopPrices;
            _inventoryRows = inventoryRows;
            _relation = relation;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(TEvent worldEvent)
        {
            foreach (var command in RelationChanged())
                yield return command;
            yield return Music();
        }

        protected IEnumerable<ViewCommand> RelationChanged()
        {
            yield return _shopPrices.RelationChanged(_relation);
            foreach (var command in _inventoryRows.Repainted())
                yield return command;
        }

        protected ViewCommand Music() => _bgm.ShopRelationChanged(_relation);
    }
}
