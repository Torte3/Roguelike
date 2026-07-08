#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Inventory;
using View.Playback;

namespace Provider.Presentations.Item
{
    internal sealed class InventoryRowsChangedPresentation : Presentation<InventoryRowsChanged>
    {
        private readonly InventoryRows _inventoryRows;

        public InventoryRowsChangedPresentation(InventoryRows inventoryRows)
        {
            _inventoryRows = inventoryRows;
        }

        protected override Guid? SubjectOf(InventoryRowsChanged worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(InventoryRowsChanged worldEvent)
        {
            yield return _inventoryRows.Changed(worldEvent.Changes);
        }
    }
}
