#nullable enable
using System;
using System.Linq;
using DG.Tweening;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Service.Events;
using Domain.Service.Items;
using Game;
using Provider.Input;
using R3;
using UnityEngine;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedIconEntityView : SynchronizedEntityView<IEntity, EntityView>, IDisposable
    {
        private readonly SerialDisposable[] _disposable =
            EnumerableExtension.CreateNewInstances<SerialDisposable>(5).ToArray();

        protected override InputReceiver _inputReceiver { get; init; }
        protected override GameManager _gameManager { get; init; }
        protected override World _world { get; init; }

        protected override EntityView GetEntityView(EntityView view)
        {
            return view;
        }

        [Inject]
        public SynchronizedIconEntityView(World world, InputReceiver inputReceiver, GameManager gameManager)
        {
            _inputReceiver = inputReceiver;
            _gameManager = gameManager;
            _world = world;

            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                mapChanged.PreviousMap?.StandaloneEntityEventEntities.ForEach(entity => Remove(entity));
                _disposable[0].Disposable = mapChanged.Map.StandaloneEntityEventEntities.SubscribeIncludingCurrentItems(Add, Remove);
            });
            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                mapChanged.PreviousMap?.StandaloneCharacterEventEntities.ForEach(entity => Remove(entity));
                _disposable[1].Disposable = mapChanged.Map.StandaloneCharacterEventEntities.SubscribeIncludingCurrentItems(Add, Remove);
            });
            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                mapChanged.PreviousMap?.StandalonePlayerEventEntities.ForEach(entity => Remove(entity));
                _disposable[2].Disposable = mapChanged.Map.StandalonePlayerEventEntities.SubscribeIncludingCurrentItems(Add, Remove);
            });
            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                mapChanged.PreviousMap?.StandaloneScheduledEventEntities.ForEach(entity => Remove(entity));
                _disposable[3].Disposable = mapChanged.Map.StandaloneScheduledEventEntities.SubscribeIncludingCurrentItems(Add, Remove);
            });
            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                mapChanged.PreviousMap?.Items.ForEach(item => Remove(item));
                _disposable[4].Disposable = mapChanged.Map.Items.SubscribeIncludingCurrentItems(Add, Remove);
            });
        }

        protected override EntityView ViewPrefab(IEntity eventEntity)
        {
            if (eventEntity is Bonfire)
            {
                return ObjectLoader.LoadPrefab("Bonfire").GetComponent<EntityView>();
            }

            if (eventEntity is Money)
            {
                return ObjectLoader.LoadPrefab("Money").GetComponent<EntityView>();
            }

            if (eventEntity is MimicMoney)
            {
                return ObjectLoader.LoadPrefab("Money").GetComponent<EntityView>();
            }

            if (eventEntity is Trap)
            {
                return ObjectLoader.LoadPrefab("Trap").GetComponent<EntityView>();
            }

            if (eventEntity is Stairs stairs)
            {
                switch (stairs.Type)
                {
                    case MovementEntityType.UpStairs:
                        return ObjectLoader.LoadPrefab("UpStairs").GetComponent<EntityView>();
                    case MovementEntityType.DownStairs:
                        return ObjectLoader.LoadPrefab("DownStairs").GetComponent<EntityView>();
                    case MovementEntityType.MagicCircle:
                        return ObjectLoader.LoadPrefab("MagicCircle").GetComponent<EntityView>();
                    default:
                        throw new NotImplementedException();
                }
            }

            if (eventEntity is MimicStairs mimicStairs)
            {
                switch (mimicStairs.Type)
                {
                    case MovementEntityType.UpStairs:
                        return ObjectLoader.LoadPrefab("UpStairs").GetComponent<EntityView>();
                    case MovementEntityType.DownStairs:
                        return ObjectLoader.LoadPrefab("DownStairs").GetComponent<EntityView>();
                    case MovementEntityType.MagicCircle:
                        return ObjectLoader.LoadPrefab("MagicCircle").GetComponent<EntityView>();
                    default:
                        throw new NotImplementedException();
                }
            }

            if (eventEntity is IItemEntity)
            {
                return ObjectLoader.LoadPrefab("Item").GetComponent<EntityView>();
            }
            else if (eventEntity is MimicItemEntity)
            {
                return ObjectLoader.LoadPrefab("Item").GetComponent<EntityView>();
            }

            if (eventEntity.Entity.Layer == EntityLayer.Middle)
            {
                return ObjectLoader.LoadPrefab("Entity").GetComponent<EntityView>();
            }
            else if (eventEntity.Entity.Layer == EntityLayer.Bottom || eventEntity.Entity.Layer == EntityLayer.Floor)
            {
                return ObjectLoader.LoadPrefab("EntityBottom").GetComponent<EntityView>();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            foreach (var disposable in _disposable)
            {
                disposable.Dispose();
            }
        }

        protected override void InitializeView(IEntity eventEntity, EntityView entityView)
        {
            var spriteView = entityView.GetComponent<SpriteView>();
            if (eventEntity is Bonfire bonfire)
            {
                var bonfireView = entityView.GetComponent<BonfireView>();
                bonfire.IsFire
                    .Subscribe(isFire => bonfireView.ShowFire(isFire))
                    .AddTo(entityView);
            }
            else if (eventEntity is MagicPot magicPot)
            {
                magicPot.CanUse
                    .Subscribe(canUse => spriteView.GetComponent<SpriteRenderer>().sprite = magicPot.Icon)
                    .AddTo(entityView);
            }
            else if (eventEntity is Workbench workbench)
            {
                workbench.CanUse
                    .Subscribe(canUse => spriteView.GetComponent<SpriteRenderer>().sprite = workbench.Icon)
                    .AddTo(entityView);
            }
            else if (eventEntity is Statue statue)
            {
                statue.OnAttacked
                    .Subscribe(_ => entityView.transform.DOShakePosition(0.5f, 0.1f))
                    .AddTo(entityView);
            }
            else if (eventEntity is IItemEntity itemEntity)
            {
                var itemView = entityView.GetComponent<ItemView>();
                itemView.SetShiny(itemEntity.Item.IsShiny);
            }
            else if (eventEntity is MimicItemEntity mimicItemEntity)
            {
                var itemView = entityView.GetComponent<ItemView>();
                itemView.SetShiny(mimicItemEntity.Item.IsShiny);
            }
            if (eventEntity is IIconEntity iconEventEntity)
            {
                spriteView.GetComponent<SpriteRenderer>().sprite = iconEventEntity.Icon;
            }
        }

        protected override void CleanupView(IEntity item, EntityView view)
        {
        }
    }
}