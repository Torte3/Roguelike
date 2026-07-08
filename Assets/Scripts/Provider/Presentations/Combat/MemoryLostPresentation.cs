#nullable enable
using Domain.Model.WorldEvents;
using Provider.Texts;
using System;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Combat
{
    internal sealed class MemoryLostPresentation : Presentation<MemoryLost>
    {
        protected override Guid? SubjectOf(MemoryLost worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(MemoryLost worldEvent)
        {
            var name = new CharacterNameText(worldEvent.Label).Text;
            yield return AddLog.Line(worldEvent.Kind switch
            {
                MemoryKind.ItemNames => $"{name}はアイテムの名前を忘れてしまった",
                MemoryKind.Characters => $"{name}は他のキャラクターのことを忘れてしまった",
                _ => throw new ArgumentOutOfRangeException(nameof(worldEvent.Kind), worldEvent.Kind, null),
            });
        }
    }
}
