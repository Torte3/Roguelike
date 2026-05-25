#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using UnityEngine;
using Utilities;

namespace Domain.Service.Items
{
    public class ItemEntity : IItemEntity
    {
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;

        public ItemEntity(ItemEntityMemento item)
        {
            Item = item.Item.Deserialize();
            Entity = new EntityBase(item.Entity);
        }

        public IItem Item { get; init; }

        public Sprite Icon => Item.Icon;

        public bool ShouldRevealMimic(IMap map)
        {
            if (Item.ShouldRevealMimic(map.Player.Character, Entity.CurrentPosition, map))
            {
                Entity.Destroy("モンスターが正体を表した");
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public ItemEntityMemento Serialize()
        {
            return new ItemEntityMemento
            (
                Item.Serialize(),
                Entity.Serialize()
            );
        }

        public static ItemEntityMemento Build(Vector2Int position, IItemMemento item)
        {
            return new ItemEntityMemento(item, EntityBase.Build(position, EntityLayer.Bottom));
        }

        public static Vector2Int GetThrowDestination(Vector2Int position, Direction8 direction, int distance, IMap map)
        {
            return map.GetThrowDestination(position, direction, distance, EntityLayer.Middle);
        }

        public static float EvaluateThrow(IItem item, Vector2Int position, IActor actor, Direction8 direction,
            int distance, IMap map)
        {
            if (!item.CanActivateWhenThrown)
                return 0;

            var destination = GetThrowDestination(position, direction, distance, map);

            return item.EvaluateWhenThrown(actor, destination, direction, map);
        }

        public async UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            var destination = GetThrowDestination(Entity.CurrentPosition, direction, distance, map);
            if (Entity.IsVisible && destination != Entity.CurrentPosition)
            {
                Entity.SetVisibility(false);
                await map.ShowThrowAnimation(Icon, Entity.CurrentPosition, direction, distance, false, EntityLayer.Middle);
                Entity.Teleport(map.FindBlankPositionFrom(destination,
                    position => map.At(position).IsBlankAndStandable(EntityLayer.Bottom)));
            }

            if (Item.CanActivateWhenThrown)
            {
                var result = await Item.UseWhenThrown(actor, destination, direction, map);
            }
        }
    }
}