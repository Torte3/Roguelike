#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class EntityMovedPresentation : EntityPresentation<EntityMoved>
    {
        protected override bool WaitsForMovers(EntityMoved worldEvent)
        {
            return worldEvent.IsVisible && worldEvent.Kind != MoveKind.Walk;
        }

        protected override IEnumerable<ViewOp> OpsOf(EntityMoved worldEvent)
        {
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
            var key = worldEvent.Entity.Key();
            var position = worldEvent.Entity.Position;
            var isVisibleAfter = worldEvent.Entity.IsVisible;
            if (!worldEvent.IsVisible)
            {
                yield return new PlaceEntity(key, position, isVisibleAfter);
                yield break;
            }

            switch (worldEvent.Kind)
            {
                case MoveKind.Walk:
                    yield return new WalkEntity(key, position, isVisibleAfter);
                    break;
                case MoveKind.Thrown:
                    yield return new ThrowEntity(key, position, isVisibleAfter);
                    break;
                case MoveKind.Teleport:
                    yield return new PlaceEntity(key, position, isVisibleAfter);
                    yield return new PauseForTeleport();
                    if (worldEvent.IsPlayer)
                        yield return new PlaySe(SeKind.Teleport);
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(worldEvent), worldEvent.Kind, null);
            }
        }
    }
}
