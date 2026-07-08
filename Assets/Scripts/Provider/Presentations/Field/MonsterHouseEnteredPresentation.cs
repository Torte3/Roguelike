#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using Provider.Texts;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Field
{
    internal sealed class MonsterHouseEnteredPresentation : Presentation<MonsterHouseEntered>
    {
        private readonly BGMSelector _bgm;

        public MonsterHouseEnteredPresentation(BGMSelector bgm)
        {
            _bgm = bgm;
        }

        protected override bool IsWaitingForMovers(MonsterHouseEntered worldEvent) => true;

        protected override IEnumerable<ViewCommand> CommandsOf(MonsterHouseEntered worldEvent)
        {
            yield return new Pause(RoomEvents.PauseSeconds);
            yield return AddLog.Line(Tint.Alert.Paint("モンスターハウスだ！"));
            yield return _bgm.MonsterHouseEntered();
        }
    }
}
