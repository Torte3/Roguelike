#nullable enable
using System;
using Domain.Service.Events;
using Game;
using Provider.Input;
using R3;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedFireEntityView : SynchronizedEntityView<Fire, EntityView>,
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
        public SynchronizedFireEntityView(World world, InputReceiver inputReceiver, GameManager gameManager)
        {
            _inputReceiver = inputReceiver;
            _gameManager = gameManager;
            _world = world;

            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                mapChanged.PreviousMap?.FireEntities.ForEach(entity => Remove(entity));
                _disposable.Disposable = mapChanged.Map.FireEntities.SubscribeIncludingCurrentItems(Add, Remove);
            });
        }

        protected override EntityView ViewPrefab(Fire _)
        {
            return ObjectLoader.LoadPrefab("Fire").GetComponent<EntityView>();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        protected override void InitializeView(Fire eventEntity, EntityView entityView)
        {
        }

        protected override void CleanupView(Fire item, EntityView view)
        {
        }
    }
}