#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class EntityDisappearedPresentation : Presentation<EntityDisappeared>
    {
        private readonly ChargePreviews _charges;
        private readonly ChestKeyHolders _keyHolders;

        public EntityDisappearedPresentation(ChargePreviews charges, ChestKeyHolders keyHolders)
        {
            _charges = charges;
            _keyHolders = keyHolders;
        }

        protected override Guid? SubjectOf(EntityDisappeared worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(EntityDisappeared worldEvent)
        {
            var key = KeyOf(worldEvent.Entity);
            _charges.EntityDisappeared(key);
            var keyHolders = _keyHolders.EntityDisappeared(key);

            var commands = new List<ViewCommand> { new RemoveEntity(key) };
            commands.AddRange(keyHolders);
            return commands;
        }
    }
}
