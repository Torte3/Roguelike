#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Events;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Items
{
    public class MimicItemEntity : ICharacterEventEntity, IIconEntity
    {
        private readonly ItemEntity _itemEntity;
        public IItem Item => _itemEntity.Item;
        public EntityBase Entity => _itemEntity.Entity;
        public EnemyData Mimic { get; init; }
        public bool IsGrounded => true;

        public MimicItemEntity(MimicItemMemento data)
        {
            _itemEntity = new ItemEntity(data.ItemEntity);
            Mimic = data.Mimic.Value;
            Event = new CharacterEvent(
                character => character.CanPickUp,
                (character, gameManager, map) =>
                {
                    Reveal(map);
                    return UniTask.CompletedTask;
                }
            );
        }

        public ICharacter Reveal(IMap map)
        {
            GameLog.Add(map.Player.Character.IsVisible(Entity.CurrentPosition), $"{Item.GetName(map.Player, map.ItemPlaceholders)}は{Mimic.Name}の擬態だった！");
            Entity.Destroy("モンスターが正体を表した");
            return map.SpawnEnemyIgnoreMimic(
                Mimic,
                Entity.CurrentPosition,
                doActImmediately: true,
                isSlept: false,
                isShiny: false
            );
        }

        public Sprite Icon => Item.Icon;

        public ICharacterEvent Event { get; init; }

        public void Dispose()
        {
            _itemEntity.Dispose();
        }

        public MimicItemMemento Serialize()
        {
            return new MimicItemMemento(_itemEntity.Serialize(), Mimic);
        }

        public static MimicItemMemento Build(ItemEntityMemento item, EnemyData mimic)
        {
            return new MimicItemMemento(item, mimic);
        }

        public async UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            var destination = ItemEntity.GetThrowDestination(Entity.CurrentPosition, direction, distance, map);
            if (Entity.IsVisible && destination != Entity.CurrentPosition)
            {
                Entity.SetVisibility(false);
                await map.ShowThrowAnimation(Icon, Entity.CurrentPosition, direction, distance, false, EntityLayer.Middle);
                Entity.Teleport(map.FindBlankPositionFrom(destination,
                    position => map.At(position).IsBlankAndStandable(EntityLayer.Bottom)));
            }
        }
    }
}