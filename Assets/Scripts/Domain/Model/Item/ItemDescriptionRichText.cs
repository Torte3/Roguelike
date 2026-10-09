using System.Text;
using System.Text.RegularExpressions;
using Domain.Model.Effect;
using UnityEngine;
using Utilities;

namespace Domain.Model.Item
{
    /// <summary>TMP 向け色タグの定数・ルール・ヘルパー。</summary>
    public static class ItemDescriptionRichText
    {
        /// <summary>確率・％表記。</summary>
        public static readonly Color ProbabilityColor = Colors.Gold;

        /// <summary>回復量など。</summary>
        public static readonly Color HealAmountColor = Colors.MediumSeaGreen;

        /// <summary>攻撃威力（属性＋数値のまとまり）。</summary>
        public static readonly Color PowerColor = Colors.Orange;

        /// <summary>距離・射程・範囲のマス数・対象体数など。</summary>
        public static readonly Color SpatialColor = Colors.SkyBlue;

        /// <summary>チャージ・クールなどターン数。</summary>
        public static readonly Color TimingColor = Colors.SlateBlue;

        /// <summary>消費HP。</summary>
        public static readonly Color HpCostColor = Colors.Crimson;

        /// <summary>残り回数・強化・繰り返し回数などメタ系。</summary>
        public static readonly Color MetaColor = Colors.LightSlateGray;

        public static readonly Color GoodColor = Colors.LimeGreen;
        public static readonly Color BadColor = Colors.Purple;

        private static string Wrap(Color color, string text) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";

        private static string Emphasize(string text) => $"<b>{text}</b>";

        public static string HeaderLine(string plainText) => Emphasize($"{plainText}");

        public static string HarmfulLine(string plainText) => Wrap(BadColor, plainText);

        private static readonly Regex PercentageToken = new(@"\d+\s*[％%]");

        /// <summary>プレーンな1行に対し、%表記だけを着色（行内に既存の &lt;color&gt; がないこと）。</summary>
        public static string ColorPercentagesInPlainText(string text) =>
            string.IsNullOrEmpty(text) ? text : PercentageToken.Replace(text, m => Wrap(ProbabilityColor, m.Value));

        public static string RichHealAmount(int value) => Wrap(HealAmountColor, value.ToString());

        internal static string RichSpatial(int value) => Wrap(SpatialColor, value.ToString());

        public static string RichSpatialCells(int value) => Wrap(SpatialColor, $"{value}マス");

        public static string RichTurns(int value) => Wrap(TimingColor, value.ToString());

        public static string RichHpCost(int value) => Wrap(HpCostColor, value.ToString());

        public static string RichMeta(int value) => Wrap(MetaColor, value.ToString());

        public static string RichMeta(string value) => Wrap(MetaColor, value);

        private static readonly Regex ColorTag = new(@"</?color[^>]*>");

        /// <summary>半角・全角の括弧付き数値ブロック（Split だと括弧が落ちる環境があるため Matches で走査）。</summary>
        private static readonly Regex PassiveParenthetical =
            new(@"\([^)]*\)|（[^）]*）");

        /// <summary>パッシブ条件名。括弧外は見出し色、括弧内は効果値（倍率・数値。確率用の黄とは別）。</summary>
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

        /// <summary>属性＋威力など、攻撃の威力表示用（数値・属性まとまり）。</summary>
        public static string RichAttackPowerSummary(string plainPowerSegment) => Wrap(PowerColor, plainPowerSegment);

        /// <summary>状態異常名のみ有利・不利色で強調（効果ブロック全体の着色とは別）。</summary>
        public static string RichBracketedConditionName(string conditionName, Impact impact)
        {
            var bracketed = $"[{conditionName}]";
            if (impact == Impact.Neutral)
                return bracketed;
            var color = impact == Impact.Beneficial ? GoodColor : BadColor;
            return Wrap(color, bracketed);
        }

        /// <summary>TMP の色タグを除いたプレーン断片（テンプレが威力部分を取り出す用）。</summary>
        public static string StripColorTags(string text) =>
            string.IsNullOrEmpty(text) ? text : ColorTag.Replace(text, "");
    }
}
