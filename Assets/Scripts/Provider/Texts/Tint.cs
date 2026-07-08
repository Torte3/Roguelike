#nullable enable
using UnityEngine;
using Utilities;

namespace Provider.Texts
{
    internal sealed record Tint(Color Color)
    {
        public static Tint Ally { get; } = new(Colors.Green);
        public static Tint Enemy { get; } = new(Colors.Red);
        public static Tint Neutral { get; } = new(Colors.SkyBlue);
        public static Tint Alert { get; } = new(Colors.Red);
        public static Tint Notice { get; } = new(UnityEngine.Color.yellow);
        public static Tint Gain { get; } = new(UnityEngine.Color.green);
        public static Tint Calm { get; } = new(Colors.DeepSkyBlue);
        public static Tint Caution { get; } = new(Colors.Yellow);
        public static Tint Danger { get; } = new(Colors.Orangered);
        public static Tint ShopItemPrice { get; } = new(Colors.MediumSeaGreen);
        public static Tint OwnItemPrice { get; } = new(Colors.LightSteelBlue);

        public string Paint(string text) => text.SetColored(Color);
    }
}
