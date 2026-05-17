using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using Unity.Logging;

namespace Domain.Service.Rooms
{
    public abstract class Room<TMemento> : ISerializable<TMemento>, IEventArea
    {
        protected bool hasEntered;
        protected bool hasEverEntered;
        public bool CanExecute { get; protected set; } = true;
        private ReactiveProperty<bool> _isInside;
        public ReadOnlyReactiveProperty<bool> IsInside => _isInside;
        private readonly ReactiveProperty<bool> _hasEverEntered;
        public ReadOnlyReactiveProperty<bool> HasEverEntered => _hasEverEntered;

        public Room(RoomMemento data, Vector2Int playerPosition)
        {
            Rect = data.Room;
            _isInside = new ReactiveProperty<bool>(Rect.Contains(playerPosition));
            hasEntered = data.HasEntered;
            hasEverEntered = data.HasEverEntered;
            _hasEverEntered = new ReactiveProperty<bool>(hasEverEntered);
        }

        public RectInt Rect { get; init; }

        public async UniTask UpdatePosition(IGameManager gameManager, IMap map, Vector2Int CurrentPosition)
        {
            if (!CanExecute)
                return;

            _isInside.Value = Rect.Contains(CurrentPosition);
            if (_isInside.Value)
            {
                await UpdateTurnIfInside(gameManager, map);
                if (!CanExecute)
                    return;
                if (!hasEntered)
                {
                    if (!hasEverEntered)
                    {
                        await FirstTimeEnter(gameManager, map);
                        hasEverEntered = true;
                        _hasEverEntered.Value = true;
                        if (!CanExecute)
                            return;
                    }

                    await EveryTimeEnter(gameManager, map);
                    hasEntered = true;
                    if (!CanExecute)
                        return;
                }
            }
            else
            {
                await UpdateTurnIfNotInside(gameManager, map);
                if (!CanExecute)
                    return;
                if (hasEntered)
                {
                    await EveryTimeExit(gameManager, map);
                    hasEntered = false;
                    if (!CanExecute)
                        return;
                }
            }
        }

        public abstract TMemento Serialize();

        protected virtual UniTask UpdateTurnIfNotInside(IGameManager gameManager, IMap map)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask UpdateTurnIfInside(IGameManager gameManager, IMap map)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask FirstTimeEnter(IGameManager gameManager, IMap map)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask EveryTimeEnter(IGameManager gameManager, IMap map)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask EveryTimeExit(IGameManager gameManager, IMap map)
        {
            return UniTask.CompletedTask;
        }
    }
}