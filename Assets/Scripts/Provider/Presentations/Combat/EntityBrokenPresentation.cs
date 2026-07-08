#nullable enable
using Domain.Model.WorldEvents;
using Provider.Texts.Labels;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Combat
{
    internal sealed class EntityBrokenPresentation : Presentation<EntityBroken>
    {
        protected override IEnumerable<ViewCommand> VisibleCommandsOf(EntityBroken worldEvent)
        {
            yield return AddLog.Line($"{EntityLabelText.Of(worldEvent.Target).Text}は破壊された");
        }
    }
}
