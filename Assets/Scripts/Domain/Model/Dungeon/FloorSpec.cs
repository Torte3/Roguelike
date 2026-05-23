using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using RandomDungeonWithBluePrint;
using UnityEngine;
using Utilities;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    public record FloorSpec(
        string SectionDataName,
        string Name,
        MapType Type,
        FieldBluePrint Field,
        ItemDatabase ItemDatabase,
        ICorrectionTable<WeaponPrefix> WeaponPrefixes,
        ICorrectionTable<IItemData> ChestItems,
        ITable<TrapData> Traps,
        ITable<StatueData> Statues,
        ITable<ShopItemData> ShopItems,
        Table<EnemyData> Enemies,
        Table<EnemyData> Npcs,
        float ShinyChance,
        float SleepChance,
        float MimicChance,
        float WeaponChanceInChest,
        bool RoundRoomCorner,
        bool CaveInOneRoom,
        float WaterChance,
        float GrassChance,
        float ShopChance,
        float MonsterHouseChance,
        float RestRoomChance,
        float LakeChance,
        float ItemAttempt,
        float MoneyAttempt,
        float MoneyAverage,
        float CharacterAttempt,
        float TrapAttempt,
        float ChestChance,
        float CursedItemChance,
        float StatueChance,
        float BonfireWeight,
        float MagicPotWeight,
        float WorkbenchWeight,
        List<EnemyData> Boss,
        List<IItemData> BossReward,
        EnemyData Clerk,
        EnemyData Mimic
    )
    {
        public bool ExistBoss => Boss.Count > 0;

        public FloorSpecMemento Serialize() => new(
            SectionDataName,
            Field.name,
            Enemies.GetItems().Select(enemy => enemy.name).ToList());

        public static FloorSpec Build(
            FloorSpecMemento memento,
            FloorData floorData,
            DungeonBluePrintData blueprint) =>
            Build(
                blueprint,
                ObjectLoader.Load<SectionData>(memento.SectionDataName),
                floorData,
                ObjectLoader.Load<FieldBluePrint>(memento.FieldBluePrintName),
                new Table<EnemyData>(
                    memento.EnemyNames.Select(name => ObjectLoader.Load<EnemyData>(name)).ToList()),
                new List<EnemyData>(),
                new List<IItemData>());

        public static FloorSpec Build(
            DungeonBluePrintData blueprint,
            SectionData sectionData,
            FloorData floorData,
            FieldBluePrint field,
            Table<EnemyData> enemies,
            List<EnemyData> boss,
            List<IItemData> bossReward) =>
            new(
                sectionData.name,
                blueprint.name,
                sectionData.Type,
                field,
                new ItemDatabase(blueprint.MasterItemDataBase, blueprint.SpawnItem),
                blueprint.WeaponPrefixes,
                blueprint.MasterItemDataBase.AllChestItems,
                blueprint.Traps,
                blueprint.Statues,
                blueprint.MasterItemDataBase.ShopItems,
                enemies,
                blueprint.Npcs,
                floorData.ShinyChance,
                floorData.SleepChance,
                floorData.MimicChance,
                floorData.WeaponChanceInChest,
                sectionData.RoundRoomCorner,
                sectionData.CaveInOneRoom,
                sectionData.WaterChance,
                sectionData.GrassChance,
                floorData.ShopChance,
                floorData.MonsterHouseChance,
                floorData.RestRoomChance,
                sectionData.LakeChance,
                floorData.ItemCount,
                floorData.MoneyCount,
                floorData.MoneyAverage,
                floorData.CharacterCount,
                sectionData.TrapCount,
                floorData.ChestChance,
                floorData.CursedItemChance,
                floorData.StatueChance,
                floorData.BonfireWeight,
                floorData.MagicPotWeight,
                floorData.WorkbenchWeight,
                boss,
                bossReward,
                sectionData.Clerk,
                sectionData.Mimic);

        private int GetCount(float trials)
        {
            return RandUtils.Binomial(trials * 2, 0.5f);
        }

        public int ItemCount() => GetCount(ItemAttempt);
        public int MoneyCount() => GetCount(MoneyAttempt);
        public int CharacterCount() => GetCount(CharacterAttempt);
        public int TrapCount() => GetCount(TrapAttempt);

        public int MoneyAmount()
        {
            return Mathf.CeilToInt(RandUtils.LogNormalFromMean(MoneyAverage, 1));
        }
    }
}
