#nullable enable
using System;
using System.Collections.Generic;

namespace View.Playback
{
    public sealed record PlaybackStep(Guid? Subject, bool IsWaitingForMovers, IReadOnlyList<ViewCommand> Commands);
}
