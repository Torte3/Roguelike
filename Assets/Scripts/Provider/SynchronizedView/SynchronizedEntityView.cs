#nullable enable
using BidirectionalMap;
using Domain.Model.Entity;
using Domain.Model.Setting;
using Domain.Service.Events;
using Game;
using Provider.Input;
using R3;
using UnityEngine;
using Utilities;
using View;

namespace Provider
{
    public abstract class SynchronizedEntityView<T, TView> where T : class, IEntity where TView : Component
    {
        private readonly BiMap<T, TView> _viewDict = new();
        protected abstract TView ViewPrefab(T obj);
        protected abstract InputReceiver _inputReceiver { get; init; }
        protected abstract World _world { get; init; }
        protected abstract GameManager _gameManager { get; init; }

        protected abstract EntityView GetEntityView(TView view);

        public virtual void Add(T obj)
        {
            var view = Object.Instantiate(ViewPrefab(obj));
            _viewDict.Add(obj, view);
            InitializeView(obj, view);
            ConstructEntity(obj, GetEntityView(view));
        }

        public virtual void Remove(T obj)
        {
            CleanupView(obj, Get(obj));
            Object.Destroy(Get(obj).gameObject);
            _viewDict.Remove(obj);
        }

        protected abstract void InitializeView(T item, TView view);
        protected abstract void CleanupView(T item, TView view);

        public T? TryGet(TView view)
        {
            return _viewDict.Reverse.ContainsKey(view) ? _viewDict.Reverse[view] : null;
        }

        public T Get(TView view)
        {
            return _viewDict.Reverse[view];
        }

        public TView? TryGet(T obj)
        {
            return _viewDict.Forward.ContainsKey(obj) ? _viewDict.Forward[obj] : null;
        }

        public TView Get(T obj)
        {
            return _viewDict.Forward[obj];
        }

        public void ConstructEntity(IEntity entity, EntityView entityView)
        {
            entityView.Construct(_inputReceiver.IsDash);
            entity.Entity.OnMove.Subscribe(move => entityView.Move(move.destination, move.direction, move.isThrown))
                .AddTo(entityView);
            entity.Entity.OnTeleport.Subscribe(teleport => entityView.Teleport(teleport)).AddTo(entityView);
            Settings.GlobalSettings.ThrowMilliseconds.Value.Subscribe(value => entityView.SetThrowMilliseconds(value)).AddTo(entityView);
            Settings.GlobalSettings.MoveMilliseconds.Value.Subscribe(value => entityView.SetMoveMilliseconds(value)).AddTo(entityView);
            Settings.GlobalSettings.DashMilliseconds.Value.Subscribe(value => entityView.SetDashMilliseconds(value)).AddTo(entityView);

            if (entity is IPlayerEventEntity playerEventEntity)
            {
                var eventArrowPrefab = ObjectLoader.LoadPrefab("EvArrow");
                var eventArrow = GameObject.Instantiate(eventArrowPrefab, entityView.transform);
                _gameManager.Turn.SubscribeIncludingCurrentValue(turn =>
                {
                    var canExecuteEvent = playerEventEntity.Events.CanExecuteEvent(_world.CurrentMap.Player, _world.CurrentMap);
                    var color = canExecuteEvent ? Color.green : Color.clear;
                    eventArrow.GetComponent<SpriteRenderer>().color = color;
                }).AddTo(entityView);
            }
            else if (entity is MimicStairs mimicStairs)
            {
                var eventArrowPrefab = ObjectLoader.LoadPrefab("EvArrow");
                GameObject.Instantiate(eventArrowPrefab, entityView.transform);
            }

            var spriteView = entityView.GetComponent<SpriteView>();
            spriteView.transform.position = new Vector3(entity.Entity.CurrentPosition.x,
                entity.Entity.CurrentPosition.y, spriteView.transform.position.z);
            entity.Entity.Visibility.Subscribe(visibility => spriteView.SetVisibility(visibility)).AddTo(spriteView);
        }
    }
}