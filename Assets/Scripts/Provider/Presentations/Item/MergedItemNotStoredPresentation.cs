#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class MergedItemNotStoredPresentation : Presentation<MergedItemNotStored>
    {
        protected override IEnumerable<ViewOp> OpsOf(MergedItemNotStored worldEvent)
        {
            yield return Logs.Line("合成したアイテムがインベントリに入れられなかった");
        }
    }
}
