#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Item
{
    internal sealed class MergedItemNotStoredPresentation : Presentation<MergedItemNotStored>
    {
        protected override IEnumerable<ViewCommand> CommandsOf(MergedItemNotStored worldEvent)
        {
            yield return AddLog.Line("合成したアイテムがインベントリに入れられなかった");
        }
    }
}
