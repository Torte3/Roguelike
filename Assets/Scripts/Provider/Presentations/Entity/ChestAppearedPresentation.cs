#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class ChestAppearedPresentation : Presentation<ChestAppeared>
    {
        private readonly ChestKeyHolders _keyHolders;

        public ChestAppearedPresentation(ChestKeyHolders keyHolders)
        {
            _keyHolders = keyHolders;
        }

        protected override Guid? SubjectOf(ChestAppeared worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(ChestAppeared worldEvent)
        {
            var key = KeyOf(worldEvent.Entity);
            _keyHolders.ChestAppeared(key, worldEvent.KeyHolders.Select(holder => holder.Value).ToList());
            yield return new AppearanceFigure(worldEvent.Appearance).AddCommandFor(worldEvent.Entity);
            yield return new SetLockCount(key, worldEvent.LockCount);
            yield return new SetInteractable(key, worldEvent.CanOpen);
        }
    }
}
