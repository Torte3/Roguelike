#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class CharacterDiedPresentation : EntityPresentation<CharacterDied>
    {
        protected override IEnumerable<ViewOp> OpsOf(CharacterDied worldEvent)
        {
            yield break;
        }
    }
}
