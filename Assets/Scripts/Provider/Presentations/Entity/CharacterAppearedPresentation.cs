#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class CharacterAppearedPresentation : EntityPresentation<CharacterAppeared>
    {
        protected override IEnumerable<ViewOp> OpsOf(CharacterAppeared worldEvent)
        {
            var looks = worldEvent.Looks;
            var label = worldEvent.Label;
            yield return new AddCharacter(worldEvent.Entity.Key(), worldEvent.Entity.Position,
                worldEvent.Appearance.IsShiny, worldEvent.Entity.IsVisible,
                new CharacterSpec(
                    looks.TypeName,
                    looks.SubtypeName,
                    looks.IsBoss,
                    looks.IsFlying,
                    label.IsPlayer,
                    label.Affiliation == AffiliationType.Enemy,
                    label.Affiliation == AffiliationType.Ally,
                    worldEvent.Direction,
                    worldEvent.Health.Hp,
                    worldEvent.Health.MaxHp,
                    worldEvent.Particles,
                    Charges.Of(worldEvent.Charge),
                    worldEvent.IsKeyHolder));
            yield return new SetInteractable(worldEvent.Entity.Key(), worldEvent.AppearsInteractable);
            foreach (var op in HealthOps.PlayerStatus(worldEvent))
                yield return op;
        }
    }
}
