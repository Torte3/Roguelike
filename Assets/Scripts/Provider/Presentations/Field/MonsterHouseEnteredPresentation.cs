#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Field
{
    internal sealed class MonsterHouseEnteredPresentation : Presentation<MonsterHouseEntered>
    {
        protected override bool WaitsForMovers(MonsterHouseEntered worldEvent) => true;

        protected override IEnumerable<ViewOp> OpsOf(MonsterHouseEntered worldEvent)
        {
            yield return new Pause(RoomEvents.PauseSeconds);
            yield return Logs.Line("モンスターハウスだ！".Paint(Tint.Alert));
            yield return new PlayBgm(BgmTrack.MonsterHouse);
        }
    }
}
