#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Logs;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;

namespace Game
{
    public class EntityManager : ISerializable<EntitiesMemento>, IDisposable
    {
        public IPlayer Player => CharacterManager?.Player;

        private CharacterManager CharacterManager { get; init; }
        private ItemManager ItemManager { get; init; }
        private EventEntityManager EventEntityManager { get; init; }
        private ThrowAnimationEntityManager ThrowAnimationEntityManager { get; init; }
        private FireEntityManager FireEntityManager { get; init; }
        private readonly ObservableList<IPlayerEventEntity> _playerEventEntities = new();
        private readonly ObservableList<IScheduledEventEntity> _scheduledEventEntities = new();
        private readonly ObservableList<IEntity> _entities = new();

        public IObservableCollection<ICharacter> Characters => CharacterManager.Characters;
        public IObservableCollection<IItemEntity> Items => ItemManager.Items;
        public IObservableCollection<IEntityEventEntity> StandaloneEntityEventEntities =>
            EventEntityManager.StandaloneEntityEventEntities;
        public IObservableCollection<ICharacterEventEntity> StandaloneCharacterEventEntities =>
            EventEntityManager.StandaloneCharacterEventEntities;
        public IObservableCollection<IPlayerEventEntity> StandalonePlayerEventEntities => EventEntityManager.StandalonePlayerEventEntities;
        public IObservableCollection<IScheduledEventEntity> StandaloneScheduledEventEntities => EventEntityManager.StandaloneScheduledEventEntities;
        public List<Stairs> Stairs => EventEntityManager.Stairs;
        public IEnumerable<ILockedEntity> LockedEntities => EventEntityManager.LockedEntities;
        public IObservableCollection<IPlayerEventEntity> PlayerEventEntities => _playerEventEntities;
        public IObservableCollection<IScheduledEventEntity> ScheduledEventEntities => _scheduledEventEntities;
        public IObservableCollection<ThrowAnimationEntity> ThrowAnimationEntities =>
            ThrowAnimationEntityManager.ThrowAnimationEntities;
        public IObservableCollection<Fire> FireEntities => FireEntityManager.FireEntities;
        public IObservableCollection<IEntity> Entities => _entities;

        private readonly Dictionary<EntityLayer, Dictionary<Vector2Int, IEntity>> _allEntityPositions = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly IMap _map;
        public EntityManager(EntitiesMemento entitiesMemento, PlayerMemento playerData, List<CharacterMemento> partyMembers, Vector2Int playerPosition, bool resetPertyPositions, CharacterControlInputReceiver receiver, IGameManager gameManager, IMap map)
        {
            _map = map;
            CharacterManager = new CharacterManager(playerData, receiver, gameManager, map);

            foreach (var character in entitiesMemento.Characters)
            {
                SpawnCharacter(character, gameManager, map);
            }

            foreach (var character in partyMembers)
            {
                if (resetPertyPositions)
                {
                    SpawnCharacter(
                    character.ReplacePosition(
                        map.FindBlankPositionFrom(
                            playerPosition,
                            position => !AllCharacterPositionsFast().Contains(position)
                        )
                        ),
                        gameManager,
                        map
                    );
                }
                else
                {
                    SpawnCharacter(character, gameManager, map);
                }
            }

            ItemManager = new ItemManager();

            foreach (var item in entitiesMemento.Items)
            {
                ItemManager.SpawnItem(item);
            }

            EventEntityManager = new EventEntityManager(entitiesMemento.EventEntities);
            ThrowAnimationEntityManager = new ThrowAnimationEntityManager();
            FireEntityManager = new FireEntityManager(entitiesMemento.Fires);

            _playerEventEntities.AddWith(Characters).AddTo(_disposables);
            _playerEventEntities.AddWith(StandalonePlayerEventEntities).AddTo(_disposables);
            _scheduledEventEntities.AddWith(StandaloneScheduledEventEntities).AddTo(_disposables);

            _entities.AddWith(Characters).AddTo(_disposables);
            _entities.AddWith(Items).AddTo(_disposables);
            _entities.AddWith(StandaloneEntityEventEntities).AddTo(_disposables);
            _entities.AddWith(StandaloneCharacterEventEntities).AddTo(_disposables);
            _entities.AddWith(StandalonePlayerEventEntities).AddTo(_disposables);
            _entities.AddWith(StandaloneScheduledEventEntities).AddTo(_disposables);
            _entities.AddWith(ThrowAnimationEntities).AddTo(_disposables);
            _entities.AddWith(FireEntities).AddTo(_disposables);
        }

        public void Dispose()
        {
            CharacterManager.Dispose();
            ItemManager.Dispose();
            StandaloneEntityEventEntities.ForEach(eventEntity => eventEntity.Dispose());
            StandaloneCharacterEventEntities.ForEach(eventEntity => eventEntity.Dispose());
            StandalonePlayerEventEntities.ForEach(eventEntity => eventEntity.Dispose());
            StandaloneScheduledEventEntities.ForEach(eventEntity => eventEntity.Dispose());
            ThrowAnimationEntities.ForEach(throwAnimationEntity => throwAnimationEntity.Dispose());
            FireEntities.ForEach(fireEntity => fireEntity.Dispose());
            _disposables.Dispose();
        }

        public EntitiesMemento Serialize()
        {
            return new EntitiesMemento(
                Characters.Except(new[] { Player.Character }).Select(character => character.Serialize()).ToList(),
                Items.Select(item => item.Serialize()).ToList(),
                EventEntityManager.Serialize(),
                FireEntityManager.Serialize()
            );
        }

        public EntitiesMemento SerializeWithoutPartyMembers(IEnumerable<ICharacter> partyMembers)
        {
            return new EntitiesMemento(
                Characters.Except(new[] { Player.Character }).Except(partyMembers).Select(character => character.Serialize()).ToList(),
                Items.Select(item => item.Serialize()).ToList(),
                EventEntityManager.Serialize(),
                FireEntityManager.Serialize()
            );
        }

        public void SetRules()
        {
            foreach (var layer in Enum.GetValues(typeof(EntityLayer)).Cast<EntityLayer>())
            {
                _allEntityPositions[layer] = new Dictionary<Vector2Int, IEntity>();
            }

            Entities.SubscribeIncludingCurrentItems(
                entity =>
                {
                    if (entity.Entity.IsVisualOnly.CurrentValue)
                        return;
                    _allEntityPositions[entity.Entity.Layer].Add(entity.Entity.CurrentPosition, entity);
                },
                entity =>
                {
                    if (entity.Entity.IsVisualOnly.CurrentValue)
                        return;
                    _allEntityPositions[entity.Entity.Layer].Remove(entity.Entity.CurrentPosition);
                }
            ).AddTo(_disposables);

            Entities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.Position.Pairwise(),
                (entity, positions) =>
                {
                    if (entity.Entity.IsVisualOnly.CurrentValue)
                        return;
                    _allEntityPositions[entity.Entity.Layer].Remove(positions.Previous);
                    _allEntityPositions[entity.Entity.Layer].Add(positions.Current, entity);
                }
            ).AddTo(_disposables);

            Entities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.IsVisualOnly.SkipLatestValueOnSubscribe(),
                (entity, _) =>
                {
                    if (entity.Entity.IsVisualOnly.CurrentValue)
                        _allEntityPositions[entity.Entity.Layer].Remove(entity.Entity.CurrentPosition);
                    else
                        _allEntityPositions[entity.Entity.Layer].Add(entity.Entity.CurrentPosition, entity);
                }
            ).AddTo(_disposables);
        }
        public async UniTask UpdateTurn(IGameManager gameManager, IMap map)
        {
            FireEntityManager.UpdateTurn(map);

            RevealMimic(FireEntities.Positions());

            var burningCharacters = Characters.In(FireEntities.Positions()).ToList();
            foreach (var character in burningCharacters)
            {
                await character.LoseHp(1, "は火に焼かれた", null);
                GameLog.Add(character.Entity.IsVisible, $"{character.GetName(Player)}は火に焼かれた");
            }

            var burningItems = Items.In(FireEntities.Positions()).ToList();
            foreach (var item in burningItems)
            {
                item.Entity.Destroy($"は灰になった");
                GameLog.Add(item.IsVisible, $"{item.Item.GetName(Player, map.ItemPlaceholders)}は灰になった");
            }

            foreach (var item in Items)
            {
                item.Item.UpdateTurn();
            }

            foreach (var scheduledEventEntity in ScheduledEventEntities)
            {
                scheduledEventEntity.Event.UpdateTurn();
                if (scheduledEventEntity.Event.CanExecuteEvent())
                    scheduledEventEntity.Event.DoEvent(gameManager, map);
            }
        }

        public ICharacter SpawnCharacter(CharacterMemento character, IGameManager gameManager, IMap map)
        {
            return CharacterManager.SpawnAlly(character, gameManager, map);
        }
        public void RemoveCharacter(ICharacter character)
        {
            CharacterManager.RemoveCharacter(character);
        }
        public IItemEntity SpawnItemFromMemento(ItemEntityMemento item)
        {
            return ItemManager.SpawnItem(item);
        }
        public IItemEntity SpawnItem(IItem item, Vector2Int position)
        {
            return ItemManager.SpawnItem(item, position);
        }
        public void SpawnMimicItem(MimicItemMemento item)
        {
            EventEntityManager.Spawn(new MimicItemEntity(item));
        }
        public void SpawnMimicMoney(MimicMoneyMemento money)
        {
            EventEntityManager.Spawn(new MimicMoney(money));
        }
        public void SpawnMimicStairs(MimicStairsMemento mimicStairs)
        {
            EventEntityManager.Spawn(new MimicStairs(mimicStairs));
        }

        public void SpawnTrap(TrapData trapData, Vector2Int position)
        {
            EventEntityManager.AddTrap(new Trap(Trap.Build(trapData, position)));
        }
        public void SpawnFire(Vector2Int position)
        {
            FireEntityManager.Add(new Fire(Fire.Build(position)));
        }
        public async UniTask<Vector2Int> ShowThrowAnimation(Sprite icon, Vector2Int position, Direction8 direction,
            int distance, bool isPiercing, IMap map, params EntityLayer[] canHitLayer)
        {
            var throwAnimationEntity = new ThrowAnimationEntity(position, icon);
            ThrowAnimationEntityManager.Add(throwAnimationEntity);
            var destination = await throwAnimationEntity.Throw(direction, map, distance, isPiercing, canHitLayer);
            throwAnimationEntity.Entity.Destroy("は演出が終わったので消えた");
            return destination;
        }
        public IItem? GetItemByIdFromWorldOrInventory(Id<IItem> id)
        {
            var itemEntity = ItemManager.Items.ById(id);
            if (itemEntity != null)
                return itemEntity.Item;
            foreach (var character in Characters)
            {
                var item = character.Inventory.AllItems.ById(id);
                if (item != null)
                    return item;
            }

            return null;
        }

        public Vector2Int? GetItemPositionByIdFromWorldOrInventory(Id<IItem> id)
        {
            var itemEntity = ItemManager.Items.ById(id);
            if (itemEntity != null)
                return itemEntity.Entity.CurrentPosition;
            foreach (var character in Characters)
            {
                var item = character.Inventory.AllItems.ById(id);
                if (item != null)
                    return character.Entity.CurrentPosition;
            }

            return null;
        }

        public IEntityEventEntity? GetEntityEventEntityFastAt(Vector2Int position, EntityLayer layer)
        {
            return GetEntityFastAt(position, layer) as IEntityEventEntity;
        }

        public ICharacterEventEntity? GetCharacterEventEntityFastAt(Vector2Int position, EntityLayer layer)
        {
            return GetEntityFastAt(position, layer) as ICharacterEventEntity;
        }

        public IPlayerEventEntity? GetPlayerEventEntityFastAt(Vector2Int position, EntityLayer layer)
        {
            return GetEntityFastAt(position, layer) as IPlayerEventEntity;
        }

        public IScheduledEventEntity? GetScheduledEventEntityFastAt(Vector2Int position, EntityLayer layer)
        {
            return GetEntityFastAt(position, layer) as IScheduledEventEntity;
        }

        public IEnumerable<IEntityEventEntity> GetEntityEventEntitiesFastAt(Vector2Int position,
            IEnumerable<EntityLayer> layers)
        {
            foreach (var layer in layers)
            {
                if (GetEntityEventEntityFastAt(position, layer) is { } entity)
                    yield return entity;
            }
        }

        public IEnumerable<IEntityEventEntity> GetEntityEventEntitiesFastAt(Vector2Int position,
            params EntityLayer[] layers) => GetEntityEventEntitiesFastAt(position, (IEnumerable<EntityLayer>)layers);

        public IEnumerable<ICharacterEventEntity> GetCharacterEventEntitiesFastAt(Vector2Int position,
            IEnumerable<EntityLayer> layers)
        {
            foreach (var layer in layers)
            {
                if (GetCharacterEventEntityFastAt(position, layer) is { } entity)
                    yield return entity;
            }
        }

        public IEnumerable<ICharacterEventEntity> GetCharacterEventEntitiesFastAt(Vector2Int position,
            params EntityLayer[] layers) => GetCharacterEventEntitiesFastAt(position, (IEnumerable<EntityLayer>)layers);

        public IEnumerable<IPlayerEventEntity> GetPlayerEventEntitiesFastAt(Vector2Int position,
            IEnumerable<EntityLayer> layers)
        {
            foreach (var layer in layers)
            {
                if (GetPlayerEventEntityFastAt(position, layer) is { } entity)
                    yield return entity;
            }
        }

        public IEnumerable<IPlayerEventEntity> GetPlayerEventEntitiesFastAt(Vector2Int position,
            params EntityLayer[] layers) => GetPlayerEventEntitiesFastAt(position, (IEnumerable<EntityLayer>)layers);

        public IEnumerable<IScheduledEventEntity> GetScheduledEventEntitiesFastAt(Vector2Int position,
            IEnumerable<EntityLayer> layers)
        {
            foreach (var layer in layers)
            {
                if (GetScheduledEventEntityFastAt(position, layer) is { } entity)
                    yield return entity;
            }
        }

        public IEnumerable<IScheduledEventEntity> GetScheduledEventEntitiesFastAt(Vector2Int position,
            params EntityLayer[] layers) => GetScheduledEventEntitiesFastAt(position, (IEnumerable<EntityLayer>)layers);

        public IEntity? GetEntityFastAt(Vector2Int position, EntityLayer layer)
        {
            return _allEntityPositions[layer].GetValueOrDefault(position);
        }

        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers)
        {
            foreach (var layer in layers)
            {
                var entity = GetEntityFastAt(position, layer);
                if (entity != null)
                    yield return entity;
            }
        }

        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position, params EntityLayer[] layers) =>
            GetEntitiesFastAt(position, (IEnumerable<EntityLayer>)layers);

        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position)
        {
            return GetEntitiesFastAt(position, Enum.GetValues(typeof(EntityLayer)).Cast<EntityLayer>());
        }
        public HashSet<Vector2Int> AllItemPositionsFast()
        {
            return ItemManager.GetAllItemPositions();
        }

        public HashSet<Vector2Int> AllCharacterPositionsFast()
        {
            return CharacterManager.GetAllCharacterPositions();
        }
        public IItemEntity? GetItemAt(Vector2Int position)
        {
            return ItemManager.GetItemAt(position);
        }
        public bool CanPickUpAt(Vector2Int position, bool canPickUpShopItem)
        {
            return ItemManager.CanPickUpAt(position, canPickUpShopItem);
        }
        public IItemEntity PickUpAt(Vector2Int position, bool canPickUpShopItem)
        {
            return ItemManager.PickUpAt(position, canPickUpShopItem);
        }
        public IItemEntity? TryPickUpAt(Vector2Int position, bool canPickUpShopItem)
        {
            return ItemManager.TryPickUpAt(position, canPickUpShopItem);
        }
        public void AttackStatue(IEnumerable<Vector2Int> positions)
        {
            foreach (var statue in EventEntityManager.Statues.In(positions).ToList())
            {
                statue.Attacked();
            }
        }
        public void RevealMimic(IEnumerable<Vector2Int> positions)
        {
            foreach (var item in ItemManager.Items.In(positions).ToList())
            {
                item.ShouldRevealMimic(_map);
            }
            foreach (var mimicItem in EventEntityManager.MimicItems.In(positions).ToList())
            {
                mimicItem.Reveal(_map);
            }
            foreach (var mimicMoney in EventEntityManager.MimicMoney.In(positions).ToList())
            {
                mimicMoney.Reveal(_map);
            }
            foreach (var mimicStairs in EventEntityManager.MimicStairs.In(positions).ToList())
            {
                mimicStairs.Reveal(_map);
            }
        }
        /// <summary>
        ///     Gets a character that follows the player when moving from one map to another.
        ///     Does not include the player themselves.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ICharacter> GetFollowingCharacters()
        {
            return Characters
                .Where(character => !character.IsPlayer)
                .Where(character => character.IsAlly(Player.Character))
                .Where(character => character.IsVisible(Player.Character.Entity.CurrentPosition));
        }
    }
}