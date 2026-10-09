#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
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


        public bool ShouldRevealMimic(IMap map)
        {
            if (Item.ShouldRevealMimic(map.Player.Character, Entity.CurrentPosition, map))
            {
                Entity.Destroy();
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

        internal static Vector2Int? GetFloorLanding(Vector2Int from, Vector2Int destination, IMap map)
        {
            return destination == from
                ? null
                : map.FindBlankPositionFrom(destination, position => map.At(position).IsBlankAndStandable(EntityLayer.Bottom));
        }

        public static float EvaluateThrow(IItem item, Vector2Int position, IActor actor, Direction8 direction,
            int distance, IMap map)
        {
            if (!item.CanActivateWhenThrown)
                return 0;

            var destination = GetThrowDestination(position, direction, distance, map);

            return item.EvaluateWhenThrown(actor, destination, direction, map);
        }

        public bool CanBeBrokenBy(BreakTargets targets) => targets.HasFlag(BreakTargets.Item);

        public EntityLabel LabelIn(IMap map)
        {
            return Item.LabelIn(map);
        }

        public WorldEvent Appeared(IMap map)
        {
            return new EntityAppeared(Entity.Ref, Entity.AppearanceOf(EntityKind.Item, Item.Icon, Item.IsShiny),
                IsUnderPlayer(map) ? new Underfoot(Item.LookIn(map)) : null);
        }

        public Underfoot? UnderfootIn(IMap map)
        {
            return IsUnderPlayer(map) ? new Underfoot(Item.LookIn(map)) : null;
        }

        public Underfoot? UnderfootAfterMove(IMap map, Vector2Int destination)
        {
            if (destination == map.Player.Character.Entity.CurrentPosition)
                return new Underfoot(Item.LookIn(map));
            return IsUnderPlayer(map) ? new Underfoot(null) : null;
        }

        public Underfoot? UnderfootAfterLeaving(IMap map)
        {
            return IsUnderPlayer(map) ? new Underfoot(null) : null;
        }

        private bool IsUnderPlayer(IMap map)
        {
            return Entity.CurrentPosition == map.Player.Character.Entity.CurrentPosition;
        }

        public async UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            var destination = GetThrowDestination(Entity.CurrentPosition, direction, distance, map);
            if (GetFloorLanding(Entity.CurrentPosition, destination, map) is { } landing)
                Entity.BlowTo(landing);

            if (Item.CanActivateWhenThrown)
            {
                var result = await Item.UseWhenThrown(actor, destination, direction, map);
            }
        }
    }
}