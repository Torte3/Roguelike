#nullable enable
using System.Collections.Generic;

namespace View.Playback
{
    public sealed record PlaybackStep(EntityKey? Subject, bool WaitsForMovers, IReadOnlyList<ViewOp> Ops);
}
