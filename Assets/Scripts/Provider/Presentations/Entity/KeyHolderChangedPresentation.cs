#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class KeyHolderChangedPresentation : EntityPresentation<KeyHolderChanged>
    {
        protected override IEnumerable<ViewOp> OpsOf(KeyHolderChanged worldEvent)
        {
            yield return new SetKeyHolder(worldEvent.Entity.Key(), worldEvent.IsKeyHolder);
        }
    }
}
