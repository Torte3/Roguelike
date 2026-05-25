#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Map;
using Utilities.Stats;

namespace Domain.Service.Events
{
    public class ScheduledEvent : IScheduledEvent
    {
        private Resource _waitTurn;
        private readonly Func<IGameManager, IMap, UniTask> _doEvent;

        public ScheduledEvent(ResourceData turn, Func<IGameManager, IMap, UniTask> doEvent)
        {
            _waitTurn = new Resource(turn);
            _doEvent = doEvent;
        }

        public ResourceData WaitTurnData => _waitTurn.GetData();

        public void UpdateTurn()
        {
            _waitTurn.Gain(1);
        }

        public bool CanExecuteEvent()
        {
            return _waitTurn.IsFull();
        }

        public async UniTask<bool> DoEvent(IGameManager gameManager, IMap map)
        {
            if (CanExecuteEvent())
            {
                await _doEvent(gameManager, map);
                _waitTurn.Set(0);
                return true;
            }

            return false;
        }
    }
}