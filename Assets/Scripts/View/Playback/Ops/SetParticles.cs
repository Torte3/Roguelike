#nullable enable
using System.Collections.Generic;
using Utilities;

namespace View.Playback.Ops
{
    public sealed record SetParticles(EntityKey Key, IReadOnlyList<ParticleType> Particles) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetParticles(Key, Particles);
        }
    }
}
