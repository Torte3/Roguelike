#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Domain.Service.Logs;

namespace Domain.Service.Rooms
{
    public class Ally : PlayerEvent
    {
        private const float GiftAffectionPerPrice = 1f / 100f;

        public Ally(ICharacter character, EnemyBehavior behavior) : base(
            null,
            new List<PlayerChoiceEvent>
            {
                new(
                    "渡す",
                    (player, map) => CanGiveItem(character, player.Character),
                    async (gameManager, map) =>
                    {
                        var player = map.Player;
                        var disabledItemIndexes = new List<int>();
                        foreach (var (i, inventoryIndex) in player.Character.Inventory.AllItemsWithIndex)
                        {
                            if (!player.Character.Inventory.CanRemove(i) || i.IsDiscardBlocked)
                            {
                                disabledItemIndexes.Add(inventoryIndex);
                            }
                        }
                        var focus = await player.Character.SelectItem("渡すアイテムを選択してください", disabledItemIndexes.ToArray());
                        if (focus.HasValue && player.Character.Inventory.HasItemAt(focus.Value, out var item))
                        {
                            if (character.Inventory.CanAddToEmpty()
                                && player.Character.Inventory.CanRemove(item)
                                && !item.IsDiscardBlocked)
                            {
                                player.Character.Inventory.Remove(item);
                                character.Inventory.AddToEmpty(item);
                                GameLog.Add(character.Entity.IsVisible,
                                    $"{character.GetName(player)}に{item.GetName(player, map.ItemPlaceholders)}を渡した。");
                                TryEquipGiftedItem(character, item, map);
                                var affectionGain = item.GetPrice(map.MarketPriceTable) * GiftAffectionPerPrice;
                                character.Affiliation.ModifyAffection(player.Character.Entity.Id, affectionGain);
                            }
                            else
                            {
                                GameLog.Add(character.Entity.IsVisible, $"{item.GetName(player, map.ItemPlaceholders)}を渡せなかった。");
                            }
                        }
                    }
                ),
                new(
                    "一緒に行動",
                    (player, map) => character.IsAlly(player.Character),
                    (gameManager, map) =>
                    {
                        behavior.BehaviorData.ChaseLeader = true;
                        behavior.BehaviorData.PrioritizeEnemiesOverLeaders = false;
                        return UniTask.CompletedTask;
                    }),
                new(
                    "敵優先",
                    (player, map) => character.IsAlly(player.Character),
                    (gameManager, map) =>
                    {
                        behavior.BehaviorData.ChaseLeader = true;
                        behavior.BehaviorData.PrioritizeEnemiesOverLeaders = true;
                        return UniTask.CompletedTask;
                    }),
                new(
                    "自由行動",
                    (player, map) => character.IsAlly(player.Character),
                    (gameManager, map) =>
                    {
                        behavior.BehaviorData.ChaseLeader = false;
                        return UniTask.CompletedTask;
                    }
                )
            }
        )
        { }

        private static bool CanGiveItem(ICharacter character, ICharacter player)
        {
            if (!character.CanReceivePlayerGift || !character.CanUseItem || !character.Inventory.HasEmptySpace())
                return false;
            if (character.IsEnemy(player))
                return false;
            return character.IsAlly(player) || character.IsNeutral(player);
        }

        private static void TryEquipGiftedItem(ICharacter character, IItem item, IMap map)
        {
            if (item is not IEquipmentToggleTarget toggleTarget)
                return;
            if (item.IsEquipped.UnwrapOr(false))
                return;

            if (toggleTarget.TryToggleEquipped(character, map))
            {
                GameLog.Add(character.Entity.IsVisible,
                    $"{character.GetName(map.Player)}は{item.GetName(map.Player, map.ItemPlaceholders)}を装備した。");
            }
        }
    }
}
