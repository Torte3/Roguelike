#nullable enable
using System;
using System.Diagnostics;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Item.Data;
using Domain.Model.WorldEvents;
using Domain.Service.Items;
using Game;
using IngameDebugConsole;
using Provider.Texts;
using Unity.Logging;
using Utilities;
using VContainer;

namespace Provider.Command
{
    public class ItemCommands
    {
        private readonly World _world;

        [Inject]
        public ItemCommands(World world)
        {
            _world = world;

            DebugLogConsole.AddCommandInstance(
                "giveItem",
                "指定した対象のインベントリに指定したアイテムを追加します。",
                "GiveItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "giveItem",
                "指定した対象のインベントリに指定したアイテムを追加します。",
                "GivePrefixedItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "giveItemPlayer",
                "プレイヤーのインベントリに指定したアイテムを追加します。",
                "GiveItemPlayer",
                this);
            DebugLogConsole.AddCommandInstance(
                "giveItemPlayer",
                "プレイヤーのインベントリに指定したアイテムを追加します。",
                "GivePrefixedItemPlayer",
                this);
            DebugLogConsole.AddCommandInstance(
                "mergeItem",
                "プレイヤーインベントリ内の指定したアイテムを指定したアイテムと統合します。",
                "MergeItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "curseItem",
                "指定したアイテムを呪われた状態にします。",
                "CurseItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "sortInventory",
                "プレイヤーのインベントリをソートします。",
                "SortInventory",
                this);
        }

        private void GiveItemPlayer(string itemName)
        {
            GivePrefixedItem("player", itemName);
        }

        private void GivePrefixedItemPlayer(string itemName, string? prefixName = null)
        {
            GivePrefixedItem("player", itemName, prefixName);
        }

        private void GiveItem(string target, string itemName)
        {
            GivePrefixedItem(target, itemName);
        }

        private void GivePrefixedItem(string target, string itemName, string? prefixName = null)
        {
            try
            {
                var character = CommandUtilities.GetTarget(target, _world.CurrentMap);
                var baseItemData = ScriptableObjectLoaderExtension.LoadItemData(itemName);
                var item = baseItemData.Match<IItem>(
                    itemData => new Item(itemData),
                    directWeaponData => new DirectWeapon(directWeaponData),
                    rangedWeaponData => new RangedWeapon(rangedWeaponData),
                    artifactData => new EquipmentItem(artifactData)
                );
                if (prefixName != null)
                {
                    var prefixData = ObjectLoader.Load<WeaponPrefix>(prefixName);
                    item = baseItemData.Match<IItem>(
                        itemData => throw new Exception($"Cannot add prefix {prefixName} to {itemName}"),
                        directWeaponData => new DirectWeapon(DirectWeapon.Build(directWeaponData, prefix: prefixData)),
                        rangedWeaponData => new RangedWeapon(RangedWeapon.Build(rangedWeaponData, prefix: prefixData)),
                        _ => throw new Exception($"Cannot add prefix {prefixName} to {itemName}")
                    );
                }

                if (character.Inventory.CanAddToEmpty())
                {
                    character.Inventory.AddToEmpty(item);
                    Log.Info($"{new ItemNameText(item.NameIn(_world.CurrentMap)).Text}を{target}のインベントリに追加しました。");
                }
                else
                {
                    Log.Info($"{target}のインベントリは一杯です。");
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void MergeItem(int index, int index2)
        {
            try
            {
                var inventory = _world.CurrentMap.Player.Character.Inventory;
                var item = inventory.GetItem(index);
                var item2 = inventory.GetItem(index2);
                if (item == null || item2 == null)
                {
                    Log.Info($"インベントリ内の指定したアイテムが見つかりません。");
                    return;
                }
                if (!item.CanBeMergeBase || !ItemMergeExtension.CanSelectForMergedItem(item2, item))
                {
                    Log.Info($"指定したアイテムは合成できません。");
                    return;
                }
                var mergedItem = item.MergeWith(item2);
                inventory.Replace(mergedItem, index);
                inventory.Remove(index2);
                var map = _world.CurrentMap;
                var character = map.Player.Character;
                _world.Events.Record(new ItemsMerged(character.Label, item.NameIn(map), item2.NameIn(map)));
                Log.Info($"{new ItemNameText(mergedItem.NameIn(_world.CurrentMap)).Text}をプレイヤーのインベントリに追加しました。");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void CurseItem(int index)
        {
            try
            {
                var player = _world.CurrentMap.Player;
                var inventory = player.Character.Inventory;
                var item = inventory.GetItem(index);
                if (item == null)
                {
                    Log.Info($"インベントリ内の指定したアイテムが見つかりません。");
                    return;
                }
                item.SetCursed(player.Character, _world.CurrentMap, true);
                Log.Info($"{new ItemNameText(item.NameIn(_world.CurrentMap)).Text}を呪われた状態にしました。");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void SortInventory(InventorySortingMode sortingMode)
        {
            var map = _world.CurrentMap;
            map.Player.Character.Inventory.Sort(sortingMode, map.MarketPriceTable);
        }
    }
}