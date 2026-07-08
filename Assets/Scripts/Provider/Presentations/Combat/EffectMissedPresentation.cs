#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Combat
{
    internal sealed class EffectMissedPresentation : EntityPresentation<EffectMissed>
    {
        protected override IEnumerable<ViewOp> OpsOf(EffectMissed worldEvent)
        {
            if (!worldEvent.IsVisible)
                yield break;

            var name = Names.Of(worldEvent.Label);
            yield return Logs.Line(worldEvent.Reason switch
            {
                EffectMissReason.CannotBeCursed => $"{name}は呪われない",
                EffectMissReason.NoItemToCurse => $"{name}は呪いの対象になるアイテムを持っていない",
                EffectMissReason.DidNotDropItem => $"{name}はアイテムを落とさなかった",
                EffectMissReason.HasNoItem => $"{name}はアイテムを持っていない",
                EffectMissReason.NoUpgradedItem => $"{name}は強化されたアイテムを持っていない",
                _ => throw new ArgumentOutOfRangeException(nameof(worldEvent), worldEvent.Reason, null),
            });
        }
    }
}
