#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Commands;
using View.Playback.Values;

namespace Provider.Presentations.Field
{
    internal sealed class FacilityUsedPresentation : Presentation<FacilityUsed>
    {
        protected override IEnumerable<ViewCommand> CommandsOf(FacilityUsed worldEvent)
        {
            yield return new PlaySE(worldEvent.Kind switch
            {
                FixtureKind.Bonfire => SEKind.BonfireRest,
                FixtureKind.Workbench => SEKind.WorkbenchCraft,
                FixtureKind.MagicPot => SEKind.MagicPotEnhance,
                FixtureKind.Chest => SEKind.OpenChest,
                FixtureKind.UpStairs or FixtureKind.DownStairs => SEKind.Stairs,
                FixtureKind.MagicCircle => SEKind.Teleport,
                _ => throw new NotImplementedException($"Facility {worldEvent.Kind} is not implemented"),
            });
        }
    }
}
