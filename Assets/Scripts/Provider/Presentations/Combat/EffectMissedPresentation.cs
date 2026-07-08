#nullable enable
using Domain.Model.WorldEvents;
using Provider.Texts;
using System;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Combat
{
    internal sealed class EffectMissedPresentation : Presentation<EffectMissed>
    {
        protected override Guid? SubjectOf(EffectMissed worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(EffectMissed worldEvent)
        {
            var name = new CharacterNameText(worldEvent.Label).Text;
            yield return AddLog.Line(worldEvent.Reason switch
            {
                EffectMissReason.CannotBeCursed => $"{name}は呪われない",
                EffectMissReason.NoItemToCurse => $"{name}は呪いの対象になるアイテムを持っていない",
                EffectMissReason.DidNotDropItem => $"{name}はアイテムを落とさなかった",
                EffectMissReason.HasNoItem => $"{name}はアイテムを持っていない",
                EffectMissReason.NoUpgradedItem => $"{name}は強化されたアイテムを持っていない",
                _ => throw new ArgumentOutOfRangeException(nameof(worldEvent.Reason), worldEvent.Reason, null),
            });
        }
    }
}
