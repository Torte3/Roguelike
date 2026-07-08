#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Commands;
using View.Playback.Values;

namespace Provider.Presentations.Entity
{
    internal sealed class ChestKeyHolderRemovedPresentation : Presentation<ChestKeyHolderRemoved>
    {
        protected override Guid? SubjectOf(ChestKeyHolderRemoved worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(ChestKeyHolderRemoved worldEvent)
        {
            var key = KeyOf(worldEvent.Entity);
            yield return new SetLockCount(key, worldEvent.LockCount);
            if (!worldEvent.CanOpen)
                yield break;

            yield return new SetInteractable(key, true);

            yield return AddLog.Line(Tint.Notice.Paint("宝箱の鍵が開いた"));
            yield return new PlaySE(SEKind.ChestUnlock);
        }
    }
}
