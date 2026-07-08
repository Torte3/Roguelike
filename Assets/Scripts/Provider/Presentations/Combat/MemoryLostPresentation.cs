#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Combat
{
    internal sealed class MemoryLostPresentation : EntityPresentation<MemoryLost>
    {
        protected override IEnumerable<ViewOp> OpsOf(MemoryLost worldEvent)
        {
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
            if (!worldEvent.IsVisible)
                yield break;

            var name = Names.Of(worldEvent.Label);
            yield return Logs.Line(worldEvent.Kind switch
            {
                MemoryKind.ItemNames => $"{name}はアイテムの名前を忘れてしまった",
                MemoryKind.Characters => $"{name}は他のキャラクターのことを忘れてしまった",
                _ => throw new ArgumentOutOfRangeException(nameof(worldEvent), worldEvent.Kind, null),
            });
        }
    }
}
