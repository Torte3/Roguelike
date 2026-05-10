#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Items;
using Domain.Service.Logs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Events
{
    public class Chest : ISerializable<ChestMemento>, IPlayerEventEntity, IIconEntity, ILockedEntity
    {
        public EntityBase Entity { get; init; }
        private List<IItem> _items;
        private Option<EnemyData> _mimic;
        public List<Id<IEntity>> KeyCharacters { get; init; }

        public Chest(ChestMemento memento)
        {
            _items = memento.Items.Select(i => i.Deserialize()).ToList();
            _mimic = memento.Mimic;
            Entity = new EntityBase(memento.Entity);
            KeyCharacters = memento.KeyCharacters;
            Events = new List<IPlayerEvent>
            {
                new PlayerEvent(
                    "宝箱を見つけた",
                    new List<PlayerChoiceEvent>
                    {
                        new(
                            "開ける",
                            (player, map) => CanExecuteEvent(map),
                            async (gameManager, map) => { await DoEvent(gameManager, map); }
                        )
                    }
                )
            };
        }

        public Sprite Icon => Addressables.LoadAssetAsync<Sprite>("Assets/Images/Monsters/ChestA.png[Chest_0]")
            .WaitForCompletion();

        public IReadOnlyList<IPlayerEvent> Events { get; init; }

        private bool CanExecuteEvent(IMap map)
        {
            return KeyCharacters.All(keyCharacterId => map.Characters.ById(keyCharacterId) == null);
        }

        private async UniTask DoEvent(IGameManager gameManager, IMap map)
        {
            gameManager.PlaySE(SE.OpenChest);
            Entity.Destroy($"は{map.Player.Character.GetName(map.Player)}に開かれた");

            IItem? selectedItem = null;

            if (_items.Count > 1)
            {
                var choices = _items.Select(item => item.GetName(map.Player, map.ItemPlaceholders)).ToArray();
                var selectedIndex = await gameManager.GetChoice("報酬を選択してください", choices);
                if (selectedIndex < 0 || selectedIndex >= _items.Count)
                    return;
                selectedItem = _items[selectedIndex];
            }
            else if (_items.Count == 1)
            {
                selectedItem = _items[0];
            }

            if (selectedItem != null)
            {
                if (map.Player.Character.Inventory.CanAddToEmpty())
                {
                    map.Player.Character.Inventory.AddToEmpty(selectedItem);
                    gameManager.RequestWorldIconPopup(selectedItem.Icon, Entity.CurrentPosition);
                    GameLog.AddIgnoreVisibility(
                        $"{map.Player.Character.GetName(map.Player)}は{selectedItem.GetName(map.Player, map.ItemPlaceholders)}を手に入れた");
                }
                else
                {
                    GameLog.AddIgnoreVisibility($"{selectedItem.GetName(map.Player, map.ItemPlaceholders)}を拾えなかった");
                    map.SpawnItem(selectedItem, Entity.CurrentPosition);
                }
            }
            else if (_mimic.IsSome(out var mimic))
            {
                map.SpawnEnemyIgnoreMimic(
                    mimic,
                    Entity.CurrentPosition,
                    doActImmediately: true,
                    isSlept: false,
                    isShiny: false
                );
            }
            else
            {
                throw new Exception("Chest has no item and mimic");
            }
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public static Vector2Int GetThrowDestination(Vector2Int position, Direction8 direction, int distance, IMap map)
        {
            var result = position;

            for (var i = 0; i < distance; i++)
            {
                if (map.At(result + direction.Vector()).CanPlace(false, false, false, EntityLayer.Middle))
                {
                    result += direction.Vector();
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        public async UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            var destination = GetThrowDestination(Entity.CurrentPosition, direction, distance, map);
            if (Entity.Visibility.CurrentValue && destination != Entity.CurrentPosition)
            {
                Entity.SetVisibility(false);
                await map.ShowThrowAnimation(Icon, Entity.CurrentPosition, direction, distance, false, EntityLayer.Middle);
                Entity.Teleport(map.FindBlankPositionFrom(destination,
                    position => map.At(position)
                        .CanPlace(false, false, false, EntityLayer.Bottom, EntityLayer.Middle)));
            }
        }

        public ChestMemento Serialize()
        {
            return new ChestMemento
            (
                _items.Select(i => i.Serialize()).ToList(),
                _mimic,
                Entity.Serialize(),
                KeyCharacters
            );
        }

        public static ChestMemento Build(IItemData item, Vector2Int position)
        {
            return Build(item.Build(), position);
        }

        public static ChestMemento Build(IItemMemento item, Vector2Int position)
        {
            return new ChestMemento
            (
                item,
                EntityBase.Build(position, EntityLayer.Middle)
            );
        }

        public static ChestMemento Build(EnemyData mimic, Vector2Int position)
        {
            return new ChestMemento
            (
                mimic,
                EntityBase.Build(position, EntityLayer.Middle)
            );
        }

        public static ChestMemento Build(List<IItemMemento> items, Vector2Int position, List<Id<IEntity>> keyCharacters)
        {
            return new ChestMemento
            (
                items,
                Option.None<EnemyData>(),
                EntityBase.Build(position, EntityLayer.Middle),
                keyCharacters
            );
        }
    }
}