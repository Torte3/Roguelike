#nullable enable
using System.Collections.Generic;
using Utilities;

namespace View.Playback.Values
{
    public sealed record CharacterAppearance(
        string TypeName,
        string SubtypeName,
        bool UsesDirectionalSprites,
        float? Scale,
        bool Walks,
        bool HasDirectionArrow,
        bool FollowsCamera,
        AffiliationMarker Marker,
        Direction8 Direction,
        int Hp,
        int MaxHp,
        IReadOnlyList<ParticleType> Particles,
        ChargePreview? Charge,
        bool IsKeyHolder);
}
