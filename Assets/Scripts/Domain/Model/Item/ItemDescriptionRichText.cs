using System.Text;
using System.Text.RegularExpressions;
using Domain.Model.Effect;

namespace Domain.Model.Item
{
    public static class ItemDescriptionRichText
    {
        public const string HeaderHex = "#C8A85C";

        public const string ProbabilityHex = "#E8C04D";

        public const string HealAmountHex = "#58D698";

        public const string PowerHex = "#E8945C";

        public const string SpatialHex = "#67B4E8";

        public const string TimingHex = "#A890E0";

        public const string HpCostHex = "#DC8068";

        public const string MetaHex = "#8FA0CC";

        public const string GoodHex = "#6CCF8A";
        public const string BadHex = "#E07070";

        public static string Wrap(string hex, string text) => $"<color={hex}>{text}</color>";

        public static string HeaderLine(string plainText) => Wrap(HeaderHex, plainText);

        public static string HarmfulLine(string plainText) => Wrap(BadHex, plainText);

        private static readonly Regex PercentageToken = new(@"\d+\s*[％%]", RegexOptions.Compiled);

        public static string ColorPercentagesInPlainText(string text) =>
            PercentageToken.Replace(text, m => Wrap(ProbabilityHex, m.Value));

        public static string RichHealAmount(int value) => Wrap(HealAmountHex, value.ToString());

        public static string RichSpatial(int value) => Wrap(SpatialHex, value.ToString());

        public static string RichTurns(int value) => Wrap(TimingHex, value.ToString());

        public static string RichHpCost(int value) => Wrap(HpCostHex, value.ToString());

        public static string RichMeta(int value) => Wrap(MetaHex, value.ToString());

        private static readonly Regex ColorTag = new(@"</?color[^>]*>", RegexOptions.Compiled);

        private static readonly Regex PassiveParenthetical =
            new(@"\([^)]*\)|（[^）]*）", RegexOptions.Compiled);

        public static string RichPassiveConditionName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
                return displayName;
            var sb = new StringBuilder();
            var last = 0;
            foreach (Match m in PassiveParenthetical.Matches(displayName))
            {
                if (m.Index > last)
                    sb.Append(Wrap(HeaderHex, displayName.Substring(last, m.Index - last)));
                sb.Append(Wrap(MetaHex, m.Value));
                last = m.Index + m.Length;
            }

            if (last < displayName.Length)
                sb.Append(Wrap(HeaderHex, displayName.Substring(last)));
            return sb.ToString();
        }

        public static string RichAttackPowerSummary(string plainPowerSegment) => Wrap(PowerHex, plainPowerSegment);

        public static string RichBracketedConditionName(string conditionName, Impact impact)
        {
            var bracketed = $"[{conditionName}]";
            if (impact == Impact.Neutral)
                return bracketed;
            var hex = impact == Impact.Beneficial ? GoodHex : BadHex;
            return Wrap(hex, bracketed);
        }

        public static string StripColorTags(string text) =>
            string.IsNullOrEmpty(text) ? text : ColorTag.Replace(text, "");

        public static string StyleEffectInfo(IEffect _, string rawFromInfo) =>
            string.IsNullOrEmpty(rawFromInfo) ? rawFromInfo : rawFromInfo;
    }
}
