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
    internal sealed class EntityMovedPresentation : Presentation<EntityMoved>
    {
        private readonly PlayerEntity _player;

        public EntityMovedPresentation(PlayerEntity player)
        {
            _player = player;
        }

        protected override Guid? SubjectOf(EntityMoved worldEvent) => KeyOf(worldEvent.Entity);

        protected override bool IsWaitingForMovers(EntityMoved worldEvent)
        {
            return worldEvent.IsVisible && worldEvent.Kind != MoveKind.Walk;
        }

        protected override IEnumerable<ViewCommand> HiddenCommandsOf(EntityMoved worldEvent)
        {
            yield return new PlaceEntity(KeyOf(worldEvent.Entity), worldEvent.Entity.Position, worldEvent.Entity.IsVisible);
        }

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(EntityMoved worldEvent)
        {
            var key = KeyOf(worldEvent.Entity);
            var position = worldEvent.Entity.Position;
            var isVisibleAfter = worldEvent.Entity.IsVisible;
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
                    if (_player.Is(worldEvent.Entity))
                        yield return new PlaySE(SEKind.Teleport);
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(worldEvent.Kind), worldEvent.Kind, null);
            }
        }
    }
}
