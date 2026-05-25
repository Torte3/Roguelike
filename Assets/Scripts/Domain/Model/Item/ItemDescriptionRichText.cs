using System.Text;
using System.Text.RegularExpressions;
using Domain.Model.Effect;
using UnityEngine;
using Utilities;

namespace Domain.Model.Item
{
    public static class ItemDescriptionRichText
    {
        public static readonly Color ProbabilityColor = Colors.Gold;

        public static readonly Color HealAmountColor = Colors.MediumSeaGreen;

        public static readonly Color PowerColor = Colors.Orange;

        public static readonly Color SpatialColor = Colors.SkyBlue;

        public static readonly Color TimingColor = Colors.SlateBlue;

        public static readonly Color HpCostColor = Colors.Crimson;

        public static readonly Color MetaColor = Colors.LightSlateGray;

        public static readonly Color GoodColor = Colors.LimeGreen;
        public static readonly Color BadColor = Colors.Purple;

        public static string Wrap(Color color, string text) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";

        public static string Emphasize(string text) => $"<b>{text}</b>";

        public static string HeaderLine(string plainText) => Emphasize($"{plainText}");

        public static string HarmfulLine(string plainText) => Wrap(BadColor, plainText);

        private static readonly Regex PercentageToken = new(@"\d+\s*[％%]");

        public static string ColorPercentagesInPlainText(string text) =>
            string.IsNullOrEmpty(text) ? text : PercentageToken.Replace(text, m => Wrap(ProbabilityColor, m.Value));

        public static string RichHealAmount(int value) => Wrap(HealAmountColor, value.ToString());

        public static string RichSpatial(int value) => Wrap(SpatialColor, value.ToString());

        public static string RichSpatialCells(int value) => Wrap(SpatialColor, $"{value}マス");

        public static string RichTurns(int value) => Wrap(TimingColor, value.ToString());

        public static string RichHpCost(int value) => Wrap(HpCostColor, value.ToString());

        public static string RichMeta(int value) => Wrap(MetaColor, value.ToString());

        public static string RichMeta(string value) => Wrap(MetaColor, value);

        private static readonly Regex ColorTag = new(@"</?color[^>]*>");

        private static readonly Regex PassiveParenthetical =
            new(@"\([^)]*\)|（[^）]*）");

        public static string RichPassiveConditionName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
                return displayName;
            var sb = new StringBuilder();
            var last = 0;
            foreach (Match m in PassiveParenthetical.Matches(displayName))
            {
                if (m.Index > last)
                    sb.Append(Emphasize(displayName.Substring(last, m.Index - last)));
                sb.Append(Wrap(MetaColor, m.Value));
                last = m.Index + m.Length;
            }

            if (last < displayName.Length)
                sb.Append(Emphasize(displayName.Substring(last)));
            return sb.ToString();
        }

        public static string RichAttackPowerSummary(string plainPowerSegment) => Wrap(PowerColor, plainPowerSegment);

        public static string RichBracketedConditionName(string conditionName, Impact impact)
        {
            var bracketed = $"[{conditionName}]";
            if (impact == Impact.Neutral)
                return bracketed;
            var color = impact == Impact.Beneficial ? GoodColor : BadColor;
            return Wrap(color, bracketed);
        }

        public static string StripColorTags(string text) =>
            string.IsNullOrEmpty(text) ? text : ColorTag.Replace(text, "");

        public static string StyleEffectInfo(IEffect _, string rawFromInfo) =>
            string.IsNullOrEmpty(rawFromInfo) ? rawFromInfo : rawFromInfo;
    }
}
