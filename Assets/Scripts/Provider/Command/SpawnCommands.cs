#nullable enable
using System;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Service.Items;
using Game;
using IngameDebugConsole;
using Unity.Logging;
using UnityEngine;
using Utilities;
using VContainer;

namespace Provider
{
    public class SpawnCommands
    {
        private readonly World _world;

        [Inject]
        public SpawnCommands(World world)
        {
            _world = world;

            DebugLogConsole.AddCommandInstance(
                "spawnItem",
                "指定した位置に指定したアイテムをスポーンします。",
                "SpawnItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "spawnItem",
                "指定した位置に指定したアイテムをスポーンします。",
                "SpawnPrefixedItem",
                this);
            DebugLogConsole.AddCommandInstance(
                "spawnEnemy",
                "指定した位置に指定した敵をスポーンします。",
                "SpawnEnemy",
                this);
        }

        private void SpawnItem(string itemName, Vector2Int position)
        {
            SpawnPrefixedItem(itemName, position);
        }

        private void SpawnPrefixedItem(string itemName, Vector2Int position, string? prefixName = null)
        {
            try
            {
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
                    if (baseItemData is DirectWeaponData weaponData)
                    {
                        var itemMemento = DirectWeapon.Build(weaponData, prefix: prefixData);
                        item = new DirectWeapon(itemMemento);
                    }
                    else
                        throw new Exception($"Cannot add prefix {prefixName} to {itemName}");
                }

                var spawnedItem = _world.CurrentMap.SpawnItem(item, position);
                var map = _world.CurrentMap;
                Log.Info($"{spawnedItem.Item.GetName(map.Player, map.ItemPlaceholders)}を{position}にスポーンしました。");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void SpawnEnemy(string enemyName, Vector2Int position, bool isSlept = false, bool isShiny = false)
        {
            try
            {
                var enemyData = ObjectLoader.Load<EnemyData>(enemyName);
                _world.CurrentMap.SpawnEnemy(
                    enemyData,
                    position,
                    doActImmediately: false,
                    isSlept: isSlept,
                    isShiny: isShiny);
                Log.Info($"{enemyData.Name}を{position}にスポーンしました。");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}