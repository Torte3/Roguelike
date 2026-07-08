#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;

namespace Domain.Service.Rooms
{
    public class Ally : PlayerEvent
    {
        private const float GiftAffectionPerPrice = 1f / 100f;

        public Ally(ICharacter character, EnemyBehavior behavior) : base(
            new List<PlayerChoiceEvent>
            {
                new(
                    "渡す",
                    (player, map) => CanGiveItem(character, player.Character),
                    (gameManager, map) => GiveItemToAlly(character, map)
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

        // プレイヤーが選んだアイテムを仲間に渡す。base(...) 初期化子から使うため static。
        // 渡せる条件（空き・取り出し可・呪い無し）を満たせば移動し、好感度を上げる。満たさなければログのみ。
        private static async UniTask GiveItemToAlly(ICharacter character, IMap map)
        {
            var player = map.Player;
            var disabledItemIndexes = new List<int>();
            foreach (var (i, inventoryIndex) in player.Character.Inventory.AllItemsWithIndex)
            {
                if (!player.Character.Inventory.CanRemove(i) || i.IsDiscardBlocked)
                    disabledItemIndexes.Add(inventoryIndex);
            }

            var focus = await player.Character.SelectItem("渡すアイテムを選択してください", disabledItemIndexes.ToArray());
            if (!focus.HasValue || !player.Character.Inventory.HasItemAt(focus.Value, out var item))
                return;

            var giving = ItemChecks.Check(character.Inventory.CanAddToEmpty()
                                          && player.Character.Inventory.CanRemove(item)
                                          && !item.IsDiscardBlocked, ItemActionFailure.CannotGive);
            if (giving.IsFailed(out var failure))
            {
                map.Events.Record(new ItemActionFailed(character.Entity.IsVisible, item.NameIn(map), failure));
                return;
            }

            player.Character.Inventory.Remove(item);
            character.Inventory.AddToEmpty(item);
            map.Events.Record(new ItemGiven(character.Entity.Ref, character.Label, item.NameIn(map), character.HeldItemsIn(map),
                player.Character.InventoryLookIn(map), map.ShopLookIn()));
            TryEquipGiftedItem(character, item, map);
            var affectionGain = item.GetPrice(map.MarketPriceTable) * GiftAffectionPerPrice;
            character.Affiliation.ModifyAffection(player.Character.Entity.Id, affectionGain);
        }

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
                map.Events.Record(new SkillUsed(character.Entity.Ref, new ItemSkillSource(character.Label,
                    item.NameIn(map), item.BaseName, item.Category, ItemUseKind.Equip)));
            }
        }
    }
}
