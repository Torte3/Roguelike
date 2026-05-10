#nullable enable
using System;
using System.Diagnostics;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Service.Items;
using Game;
using IngameDebugConsole;
using Unity.Logging;
using Utilities;
using VContainer;

namespace Provider
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
                    artifactData => new Artifact(artifactData)
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
                    var map = _world.CurrentMap;
                    Log.Info($"{item.GetName(map.Player, map.ItemPlaceholders)}を{target}のインベントリに追加しました。");
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
                if (!ItemMergeExtension.CanSelectForBaseItem(item) || !ItemMergeExtension.CanSelectForMergedItem(item2 as BaseItem, item))
                {
                    Log.Info($"指定したアイテムは合成できません。");
                    return;
                }
                var mergedItem = item.Merge(item2);
                inventory.Replace(mergedItem, index);
                inventory.Remove(index2);
                Log.Info($"{mergedItem.GetName(_world.CurrentMap.Player, _world.CurrentMap.ItemPlaceholders)}をプレイヤーのインベントリに追加しました。");
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
                item.SetCursed(player, player.Character, _world.CurrentMap.ItemPlaceholders, true);
                Log.Info($"{item.GetName(player, _world.CurrentMap.ItemPlaceholders)}を呪われた状態にしました。");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void SortInventory(InventorySortingMode sortingMode)
        {
            _world.CurrentMap.Player.Character.Inventory.Sort(sortingMode, _world.CurrentMap.MarketPriceTable);
        }
    }
}