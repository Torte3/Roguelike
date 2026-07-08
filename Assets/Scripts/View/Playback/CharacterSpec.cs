#nullable enable
using System.Collections.Generic;
using Utilities;

namespace View.Playback
{
    public sealed record CharacterSpec(
        string TypeName,
        string SubtypeName,
        bool IsBoss,
        bool IsFlying,
        bool IsPlayer,
        bool IsEnemy,
        bool IsAlly,
        Direction8 Direction,
        int Hp,
        int MaxHp,
        IReadOnlyList<ParticleType> Particles,
        ChargePreview? Charge,
        bool IsKeyHolder);
}
