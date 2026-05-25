#nullable enable

namespace Domain.Model.Item
{
    public enum Rarity
    {
        Common, //基準: 普通、ありふれたもの。感覚としてはだいたいこれ
        Uncommon, //基準: 少し変わった特徴がある。感覚としてはたまにこれ
        Rare, //基準: 珍しい特徴、強さがある。感覚としては引いたら嬉しい
        Epic, //基準: めったに見ない。感覚としては狙って出すもの
        Legendary //基準: 文句なしに強い。感覚としては1度出るかどうか
    }
}