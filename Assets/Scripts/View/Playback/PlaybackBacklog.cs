#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace View.Playback
{
    public sealed class PlaybackBacklog
    {
        private readonly Queue<PlaybackStep> _steps = new();

        internal bool IsEmpty => _steps.Count == 0;

        internal void Enqueue(PlaybackStep step)
        {
            _steps.Enqueue(step);
        }

        internal PlaybackStep Peek()
        {
            return _steps.Peek();
        }

        internal PlaybackStep Dequeue()
        {
            return _steps.Dequeue();
        }

        internal int CountWalksBeforeMoverWait(Guid subject)
        {
            return _steps
                .TakeWhile(step => !step.IsWaitingForMovers)
                .Count(step => step.Commands.Any(command => command.IsWalkOf(subject)));
        }
    }
}
