#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Field
{
    internal sealed class FacilityUsedPresentation : Presentation<FacilityUsed>
    {
        protected override IEnumerable<ViewOp> OpsOf(FacilityUsed worldEvent)
        {
            yield return new PlaySe(worldEvent.Kind switch
            {
                FixtureKind.Bonfire => SeKind.BonfireRest,
                FixtureKind.Workbench => SeKind.WorkbenchCraft,
                FixtureKind.MagicPot => SeKind.MagicPotEnhance,
                FixtureKind.Chest => SeKind.OpenChest,
                FixtureKind.UpStairs or FixtureKind.DownStairs => SeKind.Stairs,
                FixtureKind.MagicCircle => SeKind.Teleport,
                _ => throw new NotImplementedException($"Facility {worldEvent.Kind} is not implemented"),
            });
        }
    }
}
