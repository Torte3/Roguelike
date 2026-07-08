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
using Domain.Model.WorldEvents;
using Domain.Service.Effect;
using Domain.Service.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Events
{
    public class Chest : ISerializable<ChestMemento>, IPlayerEventEntity, ILockedEntity
    {
        public EntityBase Entity { get; init; }
        public bool IsGrounded => true;
        private List<IItem> _items;
        private Option<EnemyData> _mimic;
        private readonly List<Id<IEntity>> _keyHolders;
        public bool HasLock { get; }

        public Chest(ChestMemento memento)
        {
            _items = memento.Items.Select(i => i.Deserialize()).ToList();
            _mimic = memento.Mimic;
            Entity = new EntityBase(memento.Entity);
            HasLock = memento.HasLock;
            _keyHolders = memento.KeyHolders;
            Events = new List<IPlayerEvent>
            {
                new PlayerEvent(
                    FixtureKind.Chest,
                    new List<PlayerChoiceEvent>
                    {
                        new(
                            "開ける",
                            (player, map) => !IsLocked,
                            async (gameManager, map) => { await DoEvent(gameManager, map); }
                        )
                    }
                )
            };
        }

        private Sprite Icon => Addressables.LoadAssetAsync<Sprite>("Assets/Images/Monsters/ChestA.png[Chest_0]")
            .WaitForCompletion();

        public IReadOnlyList<IPlayerEvent> Events { get; init; }

        private bool IsLocked => HasLock && _keyHolders.Count > 0;
        public bool IsLockReleased => HasLock && _keyHolders.Count == 0;

        public bool IsKeyHolder(Id<IEntity> id)
        {
            return _keyHolders.Contains(id);
        }

        public void ForgetKeyHolder(Id<IEntity> id)
        {
            if (!_keyHolders.Remove(id))
                return;

            Entity.Record(new ChestLockReleased(Entity.Ref, _keyHolders.Count, !IsLocked));
        }

        public void LeaveMap(IMap map)
        {
            ReleaseKeyHolders(map);
        }

        private void ReleaseKeyHolders(IMap map)
        {
            foreach (var holder in map.Characters.Where(character => _keyHolders.Contains(character.Entity.Id)))
                holder.Entity.Record(new KeyHolderChanged(holder.Entity.Ref, false));
            _keyHolders.Clear();
        }

        public bool CanBeBrokenBy(BreakTargets targets) => targets.HasFlag(BreakTargets.Chest);

        public EntityLabel LabelIn(IMap map)
        {
            return new KindEntityLabel(FixtureKind.Chest);
        }

        public WorldEvent Appeared(IMap map)
        {
            return new ChestAppeared(Entity.Ref, Entity.AppearanceOf(FixtureKind.Chest.ToEntityKind(), Icon), _keyHolders.Count, !IsLocked);
        }

        private async UniTask DoEvent(IGameManager gameManager, IMap map)
        {
            map.Events.Record(new FacilityUsed(FixtureKind.Chest));
            Entity.Destroy();

            IItem? selectedItem = null;

            if (_items.Count > 1)
            {
                var selectedIndex = await gameManager.GetChoiceWithItemPreview(new ChoiceMessage("報酬を選択してください"), map, _items.ToArray());
                if (selectedIndex >= _items.Count)
                    return;
                selectedItem = _items[selectedIndex];
            }
            else if (_items.Count == 1)
            {
                selectedItem = _items[0];
            }

            if (selectedItem != null)
            {
                if (map.Player.Character.Inventory.PickUpCheck().IsFailed(out var failure))
                {
                    map.Events.Record(new FacilityItemFailed(selectedItem.NameIn(map), failure));
                    map.SpawnItem(selectedItem, Entity.CurrentPosition);
                }
                else
                {
                    map.Player.Character.Inventory.AddToEmpty(selectedItem);
                    map.Events.Record(new ItemObtainedFromChest(map.Player.Character.Label, selectedItem.NameIn(map),
                        new ObtainedItem(selectedItem.Icon, Entity.CurrentPosition), map.Player.Character.InventoryLookIn(map)));
                }
            }
            else if (_mimic.IsSome(out var mimic))
            {
                map.Events.Record(new MimicRevealed(Entity.IsVisible, LabelIn(map), mimic.Name, null));
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
            var start = Entity.CurrentPosition;
            var destination = GetThrowDestination(start, direction, distance, map);
            var remaining = distance - BlowAwayCollision.CountStepsMoved(start, destination, direction);
            if (remaining > 0)
            {
                var collisionPos = destination + direction.Vector();
                BlowAwayCollisionSide? blocker = !map.At(collisionPos).IsPassableOnMap()
                    ? BlowAwayCollisionSide.Wall()
                    : BlowAwayCollisionSide.FromEntity(map.GetEntityFastAt(collisionPos, EntityLayer.Middle));
                if (blocker.HasValue)
                {
                    await BlowAwayCollision.Apply(
                        BlowAwayCollisionSide.OtherRigidObject(),
                        blocker.Value,
                        remaining,
                        actor as ICharacter,
                        map);
                }
            }

            if (destination != Entity.CurrentPosition)
                Entity.BlowTo(map.FindBlankPositionFrom(destination, position => map.At(position)
                    .CanPlace(false, false, false, EntityLayer.Bottom, EntityLayer.Floor, EntityLayer.Middle)));
        }

        public ChestMemento Serialize()
        {
            return new ChestMemento
            (
                _items.Select(i => i.Serialize()).ToList(),
                _mimic,
                Entity.Serialize(),
                HasLock,
                _keyHolders
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

        public static ChestMemento Build(List<IItemMemento> items, Vector2Int position, List<Id<IEntity>> keyHolders)
        {
            return new ChestMemento
            (
                items,
                Option.None<EnemyData>(),
                EntityBase.Build(position, EntityLayer.Middle),
                keyHolders.Any(),
                keyHolders
            );
        }
    }
}