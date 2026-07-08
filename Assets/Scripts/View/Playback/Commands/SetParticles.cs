#nullable enable
using System;
using System.Collections.Generic;
using Utilities;

namespace View.Playback.Commands
{
    public sealed record SetParticles(Guid Key, IReadOnlyList<ParticleType> Particles) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetParticles(Key, Particles);
        }
    }
}
