#nullable enable
using UnityEngine;

namespace View.Playback.Values
{
    public sealed record AffiliationMarker(Color GroupColor, Color MinimapColor)
    {
        public static AffiliationMarker Enemy { get; } = new(new Color(1, 0, 0, 0.5f), new Color(1, 0, 0, 1f));
        public static AffiliationMarker Ally { get; } = new(new Color(0, 1, 0, 0.5f), new Color(0, 1, 0, 1f));
        public static AffiliationMarker Neutral { get; } = new(Color.clear, new Color(1, 1, 1, 1f));
    }
}
