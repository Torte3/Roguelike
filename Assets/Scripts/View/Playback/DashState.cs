#nullable enable
using System;

namespace View.Playback
{
    public sealed class DashState
    {
        public Func<bool> IsDashing { get; set; } = () => false;
    }
}
