#nullable enable
using System;
using Domain.Model.WorldEvents;
using View.Playback;

namespace Provider.Presentations
{
    internal interface IWorldEventPresentation
    {
        public Type EventType { get; }
        public PlaybackStep Present(WorldEvent worldEvent);
    }
}
