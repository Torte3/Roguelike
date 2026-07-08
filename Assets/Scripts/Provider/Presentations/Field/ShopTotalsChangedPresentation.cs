#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;

namespace Provider.Presentations.Field
{
    internal sealed class ShopTotalsChangedPresentation : Presentation<ShopTotalsChanged>
    {
        private readonly ShopPrices _shopPrices;

        public ShopTotalsChangedPresentation(ShopPrices shopPrices)
        {
            _shopPrices = shopPrices;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(ShopTotalsChanged worldEvent)
        {
            yield return _shopPrices.TotalsChanged(worldEvent);
        }
    }
}
