#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class ChestAppearedPresentation : EntityPresentation<ChestAppeared>
    {
        protected override IEnumerable<ViewOp> OpsOf(ChestAppeared worldEvent)
        {
            var key = worldEvent.Entity.Key();
            var appearance = worldEvent.Appearance;
            yield return new AddEntity(key, Prefabs.NameOf(appearance), worldEvent.Entity.Position,
                appearance.Icon, appearance.IsShiny, worldEvent.Entity.IsVisible);
            yield return new SetLockCount(key, worldEvent.LockCount);
            yield return new SetInteractable(key, worldEvent.CanOpen);
        }
    }
}
