#nullable enable
using System;
using UnityEngine;
using Utilities;

namespace Domain.Model.Item
{
    public static class RarityExtensions
    {
        /// <summary>
        /// Get weight of rarity
        /// </summary>
        /// <param name="rarity"></param>
        /// <param name="progress">0~1 </param>
        /// <returns></returns>
        public static float GetWeight(this Rarity rarity, float progress)
        {
            progress = Mathf.Clamp01(progress);

            float uncommon = 27f + 3f * progress;
            float rare = 1f + 24f * Mathf.Pow(progress, 1.7f);
            float epic = 9.5f * Mathf.Pow(progress, 2.5f);
            float legendary = 0.5f * Mathf.Pow(progress, 5f);

            float common = 100f - uncommon - rare - epic - legendary;

            return Mathf.Max(rarity switch
            {
                Rarity.Common => common,
                Rarity.Uncommon => uncommon,
                Rarity.Rare => rare,
                Rarity.Epic => epic,
                Rarity.Legendary => legendary,
                _ => 0f
            }, 0f);
        }

        public static Color GetColor(this Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => Colors.White,
                Rarity.Uncommon => Colors.Green,
                Rarity.Rare => Colors.SkyBlue,
                Rarity.Epic => Colors.Purple,
                Rarity.Legendary => Colors.Gold,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}