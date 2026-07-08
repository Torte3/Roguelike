using System;
using UnityEngine;

namespace View.Map
{
    public static class TileVisibilityExtension
    {
        public static Color GetColor(this TileVisibility visibility)
        {
            return visibility switch
            {
                TileVisibility.Visible => Color.white,
                TileVisibility.Translucent => new Color(0.5f, 0.5f, 0.5f, 1f),
                TileVisibility.Transparent => new Color(0f, 0f, 0f, 0f),
                _ => throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null)
            };
        }

        public static Color GetMinimapColor(this TileVisibility visibility)
        {
            return visibility switch
            {
                TileVisibility.Visible => new Color(1, 1, 1, 0.8f),
                TileVisibility.Translucent => new Color(1, 1, 1, 0.8f),
                TileVisibility.Transparent => new Color(0f, 0f, 0f, 0f),
                _ => throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null)
            };
        }
    }
}