#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Tile;
using Provider.Presentations.Translation;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Map
{
    internal sealed class SightChangedPresentation : Presentation<SightChanged>
    {
        private readonly PlayerSight _sight;
        private readonly TileVisibilities _tileVisibilities;
        private readonly ChargePreviews _charges;

        public SightChangedPresentation(PlayerSight sight, TileVisibilities tileVisibilities,
            ChargePreviews charges)
        {
            _sight = sight;
            _tileVisibilities = tileVisibilities;
            _charges = charges;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(SightChanged worldEvent)
        {
            var changed = _sight.Changed(worldEvent.VisibleArea);
            yield return new SetTileVisibilities(_tileVisibilities.Of(changed));
            foreach (var command in _charges.SightChanged(changed))
                yield return command;
        }
    }
}
