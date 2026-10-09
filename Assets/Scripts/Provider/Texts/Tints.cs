#nullable enable
using System;
using UnityEngine;
using Utilities;

namespace Provider.Texts
{
    internal static class Tints
    {
        public static string Paint(this string text, Tint tint)
        {
            return text.SetColored(ColorOf(tint));
        }

        private static Color ColorOf(Tint tint)
        {
            return tint switch
            {
                Tint.Ally => Colors.Green,
                Tint.Enemy => Colors.Red,
                Tint.Neutral => Colors.SkyBlue,
                Tint.Alert => Colors.Red,
                Tint.Notice => Color.yellow,
                Tint.Gain => Color.green,
                Tint.Calm => Colors.DeepSkyBlue,
                Tint.Caution => Colors.Yellow,
                Tint.Danger => Colors.Orangered,
                Tint.ShopItemPrice => Colors.MediumSeaGreen,
                Tint.OwnItemPrice => Colors.LightSteelBlue,
                _ => throw new ArgumentOutOfRangeException(nameof(tint), tint, null),
            };
        }
    }
}
