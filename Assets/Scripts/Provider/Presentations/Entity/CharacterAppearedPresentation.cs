#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;
using View.Playback.Commands;
using View.Playback.Values;

namespace Provider.Presentations.Entity
{
    internal sealed class CharacterAppearedPresentation : Presentation<CharacterAppeared>
    {
        private readonly ChargePreviews _charges;
        private readonly PlayerEntity _player;
        private readonly AffiliationMarkers _markers;
        private readonly HealthCommands _healthCommands;

        public CharacterAppearedPresentation(ChargePreviews charges, PlayerEntity player,
            AffiliationMarkers markers, HealthCommands healthCommands)
        {
            _charges = charges;
            _player = player;
            _markers = markers;
            _healthCommands = healthCommands;
        }

        protected override Guid? SubjectOf(CharacterAppeared worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(CharacterAppeared worldEvent)
        {
            var looks = worldEvent.Looks;
            var label = worldEvent.Label;
            var isPlayer = _player.Is(worldEvent.Entity);
            yield return new AddCharacter(KeyOf(worldEvent.Entity), worldEvent.Entity.Position,
                worldEvent.Appearance.IsShiny, worldEvent.Entity.IsVisible,
                new CharacterAppearance(
                    looks.TypeName,
                    looks.SubtypeName,
                    looks.TypeName == "Human",
                    looks.IsBoss ? 1.5f : null,
                    !looks.IsFlying,
                    isPlayer,
                    isPlayer,
                    _markers.Of(label.Affiliation),
                    worldEvent.Direction,
                    worldEvent.Health.Hp,
                    worldEvent.Health.MaxHp,
                    worldEvent.Particles,
                    _charges.CharacterAppeared(KeyOf(worldEvent.Entity), worldEvent.Charge),
                    worldEvent.IsKeyHolder));
            yield return new SetInteractable(KeyOf(worldEvent.Entity), worldEvent.AppearsInteractable);
            foreach (var command in _healthCommands.PlayerStatus(worldEvent.Entity, worldEvent.Health))
                yield return command;
        }
    }
}
