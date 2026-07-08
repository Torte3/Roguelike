#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Effect
{
    public record EffectArea(IReadOnlyList<Vector2Int> Positions, Color Color);
}
