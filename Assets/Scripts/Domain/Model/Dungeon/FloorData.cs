#nullable enable
using System;
using RandomDungeonWithBluePrint;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Dungeon
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Floor")]
    public class FloorData : ScriptableObject
    {
        public FieldBluePrint? Field;
        [Range(0, 1)] public float ShinyChance = 0.01f;
        [Range(0, 1)] public float SleepChance = 0.75f;
        [Range(0, 1)] public float MimicChance = 0.1f;
        [Range(0, 1)] public float ShopChance = 0.05f;
        [Range(0, 1)] public float MonsterHouseChance = 0.05f;
        [Range(0, 1)] public float RestRoomChance = 0.2f;
        [MinValue(0)] public float ItemCount = 2;
        [MinValue(0)] public float MoneyCount = 1;
        [MinValue(0)] public float MoneyAverage = 100;
        [MinValue(0)] public float CharacterCount = 1;
        [Range(0, 1)] public float ChestChance = 0.1f;
        [Range(0, 1)] public float WeaponChanceInChest;
        [Range(0, 1)] public float StatueChance = 0.1f;
        [Range(0, 1)] public float BonfireWeight = 1f;
        [Range(0, 1)] public float MagicPotWeight = 1f;
        [Range(0, 1)] public float WorkbenchWeight = 1f;
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
            WeaponChanceInChest = 0f;
            StatueChance = 0.1f;
            BonfireWeight = 1f;
            MagicPotWeight = 1f;
            WorkbenchWeight = 1f;
        }
#endif
    }
}