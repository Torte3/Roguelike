#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Effect;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;
using View.Playback.Commands;
using View.Playback.Values;

namespace Provider.Presentations.Map
{
    internal sealed class AreaEffectAppliedPresentation : Presentation<AreaEffectApplied>
    {
        private readonly PlayerSight _sight;

        public AreaEffectAppliedPresentation(PlayerSight sight)
        {
            _sight = sight;
        }

        protected override bool IsWaitingForMovers(AreaEffectApplied worldEvent)
        {
            return worldEvent.IsVisible;
        }

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(AreaEffectApplied worldEvent)
        {
            yield return new PauseForAreaEffect();
            foreach (var command in EffectCommandsOf(worldEvent))
                yield return command;
        }

        protected override IEnumerable<ViewCommand> HiddenCommandsOf(AreaEffectApplied worldEvent) =>
            EffectCommandsOf(worldEvent);

        private IEnumerable<ViewCommand> EffectCommandsOf(AreaEffectApplied worldEvent)
        {
            yield return new SpawnAreaEffect(_sight.VisibleOf(worldEvent.Area), worldEvent.Color);

            var impacts = worldEvent.Hits.Where(hit => hit.Target.IsVisible).Select(hit => hit.Impact).ToHashSet();
            if (impacts.Contains(Impact.Harmful))
                yield return new PlaySE(SEKind.Attack);
            if (impacts.Contains(Impact.Beneficial))
                yield return new PlaySE(SEKind.Heal);
        }
    }
}
