#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace View.Playback.Values
{
    public sealed record ChargePreview(IReadOnlyList<Vector2Int> Area, Color Color, int Turns);
}
