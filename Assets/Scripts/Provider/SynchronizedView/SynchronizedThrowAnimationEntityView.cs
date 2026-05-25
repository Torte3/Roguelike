#nullable enable
using System;
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
    public class SynchronizedThrowAnimationEntityView : SynchronizedEntityView<ThrowAnimationEntity, EntityView>,
        IDisposable
    {
        private readonly SerialDisposable _disposable = new();
        protected override InputReceiver _inputReceiver { get; init; }
        protected override GameManager _gameManager { get; init; }
        protected override World _world { get; init; }

        protected override EntityView GetEntityView(EntityView view)
        {
            return view;
        }

        [Inject]
        public SynchronizedThrowAnimationEntityView(World world, InputReceiver inputReceiver, GameManager gameManager)
        {
            _inputReceiver = inputReceiver;
            _gameManager = gameManager;
            _world = world;

            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                mapChanged.PreviousMap?.ThrowAnimationEntities.ForEach(entity => Remove(entity));
                _disposable.Disposable = mapChanged.Map.ThrowAnimationEntities.SubscribeIncludingCurrentItems(Add, Remove);
            });
        }

        protected override EntityView ViewPrefab(ThrowAnimationEntity _)
        {
            return ObjectLoader.LoadPrefab("Entity").GetComponent<EntityView>();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        protected override void InitializeView(ThrowAnimationEntity eventEntity, EntityView entityView)
        {
            var spriteView = entityView.GetComponent<SpriteView>();
            spriteView.GetComponent<SpriteRenderer>().sprite = eventEntity.Icon;
        }

        protected override void CleanupView(ThrowAnimationEntity item, EntityView view)
        {
        }
    }
}