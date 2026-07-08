#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Inventory;
using View.Playback;

namespace Provider.Presentations.Item
{
    internal sealed class PlayerUnderfootChangedPresentation : Presentation<PlayerUnderfootChanged>
    {
        private readonly InventoryRows _inventoryRows;

        public PlayerUnderfootChangedPresentation(InventoryRows inventoryRows)
        {
            _inventoryRows = inventoryRows;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(PlayerUnderfootChanged worldEvent)
        {
            yield return _inventoryRows.UnderfootChanged(worldEvent.Underfoot);
        }
    }
}
