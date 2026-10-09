#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using Domain.Service.Events;
using Utilities;

namespace Domain.Service.Items
{
    public class MimicItemEntity : ICharacterEventEntity
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
            map.Events.Record(new MimicRevealed(Entity.IsVisible, LabelIn(map), Mimic.Name, null));
            Entity.Destroy();
            return map.SpawnEnemyIgnoreMimic(
                Mimic,
                Entity.CurrentPosition,
                doActImmediately: true,
                isSlept: false,
                isShiny: false
            );
        }

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

        public EntityLabel LabelIn(IMap map)
        {
            return _itemEntity.LabelIn(map);
        }

        public WorldEvent Appeared(IMap map)
        {
            return _itemEntity.Appeared(map);
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            var destination = ItemEntity.GetThrowDestination(Entity.CurrentPosition, direction, distance, map);
            if (ItemEntity.GetFloorLanding(Entity.CurrentPosition, destination, map) is { } landing)
                Entity.BlowTo(landing);

            return UniTask.CompletedTask;
        }
    }
}