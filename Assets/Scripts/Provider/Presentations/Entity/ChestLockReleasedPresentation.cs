#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class ChestLockReleasedPresentation : EntityPresentation<ChestLockReleased>
    {
        protected override IEnumerable<ViewOp> OpsOf(ChestLockReleased worldEvent)
        {
            var key = worldEvent.Entity.Key();
            yield return new SetLockCount(key, worldEvent.LockCount);
            if (!worldEvent.CanOpen)
                yield break;

            yield return new SetInteractable(key, true);

            yield return Logs.Line("宝箱の鍵が開いた".Paint(Tint.Notice));
            yield return new PlaySe(SeKind.ChestUnlock);
        }
    }
}
