#nullable enable
using RandomDungeonWithBluePrint;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Dungeon
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Floor")]
    public class FloorData : ScriptableObject
    {
        public FieldBluePrint? Field;

        [TitleGroup("確率・重み")]
        [InfoBox("マップ生成時にフロア全体で1回だけ判定する。")]
        [FoldoutGroup("確率・重み/1フロアあたり")]
        [Range(0, 1)] public float ShopChance = 0.05f;

        [FoldoutGroup("確率・重み/1フロアあたり")]
        [Range(0, 1)] public float MonsterHouseChance = 0.05f;

        [FoldoutGroup("確率・重み/1フロアあたり")]
        [Range(0, 1)] public float RestRoomChance = 0.2f;

        [FoldoutGroup("確率・重み/1フロアあたり")]
        [Range(0, 1)] public float ShinyChance = 0.01f;

        [InfoBox("通常部屋・湖部屋ごとに判定する。店・モンスターハウス・休憩ルーム・孤立部屋では判定しない。")]
        [FoldoutGroup("確率・重み/1部屋あたり")]
        [Range(0, 1)] public float ChestChance = 0.1f;

        [FoldoutGroup("確率・重み/1部屋あたり")]
        [Range(0, 1)] public float StatueChance = 0.1f;

        [InfoBox("敵・NPC・アイテム・宝箱など、個体ごとに判定する。")]
        [FoldoutGroup("確率・重み/個体ごと")]
        [Range(0, 1)] public float SleepChance = 0.75f;

        [FoldoutGroup("確率・重み/個体ごと")]
        [Range(0, 1)] public float MimicChance = 0.1f;

        [FoldoutGroup("確率・重み/個体ごと")]
        [Range(0, 1)] public float CursedItemChance = 0.05f;

        [InfoBox("RestRoomChance が成功して休憩ルームが生成されたとき、施設種別を決める相対重み。")]
        [FoldoutGroup("確率・重み/休憩ルーム1個あたり")]
        [Range(0, 1)] public float BonfireWeight = 1f;

        [FoldoutGroup("確率・重み/休憩ルーム1個あたり")]
        [Range(0, 1)] public float MagicPotWeight = 1f;

        [FoldoutGroup("確率・重み/休憩ルーム1個あたり")]
        [Range(0, 1)] public float WorkbenchWeight = 1f;

        [TitleGroup("配置量")]
        [InfoBox("通常部屋・湖部屋・特殊部屋ごとに、二項分布で個数が決まる平均値。")]
        [MinValue(0)] public float ItemCount = 2;

        [TitleGroup("配置量")]
        [MinValue(0)] public float MoneyCount = 1;

        [TitleGroup("配置量")]
        [MinValue(0)] public float MoneyAverage = 100;

        [TitleGroup("配置量")]
        [MinValue(0)] public float CharacterCount = 1;

#if UNITY_EDITOR
        [Button]
        public void SetDefault()
        {
            Field = null;
            ShinyChance = 0.01f;
            SleepChance = 0.75f;
            MimicChance = 0.1f;
            ShopChance = 0.05f;
            MonsterHouseChance = 0.05f;
            RestRoomChance = 0.2f;
            ItemCount = 2;
            MoneyCount = 1;
            MoneyAverage = 100;
            CharacterCount = 1;
            ChestChance = 0.1f;
            CursedItemChance = 0.05f;
            StatueChance = 0.1f;
            BonfireWeight = 1f;
            MagicPotWeight = 1f;
            WorkbenchWeight = 1f;
        }
#endif
    }
}
