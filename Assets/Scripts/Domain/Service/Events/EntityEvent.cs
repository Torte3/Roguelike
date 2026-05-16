using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Entity;
using Domain.Model.Map;

namespace Domain.Service.Events
{
    public class EntityEvent : IEntityEvent
    {
        private readonly Func<IEntity, bool> _canExecuteEvent;
        private readonly Func<IEntity, IGameManager, IMap, UniTask> _doEvent;

        public EntityEvent(Func<IEntity, bool> canExecuteEvent,
            Func<IEntity, IGameManager, IMap, UniTask> doEvent)
        {
            _canExecuteEvent = canExecuteEvent;
            _doEvent = doEvent;
        }

        public bool CanExecuteEvent(IEntity entity)
        {
            return _canExecuteEvent(entity);
        }

        public async UniTask<bool> DoEvent(IEntity entity, IGameManager gameManager, IMap map)
        {
            if (_canExecuteEvent(entity))
            {
                await _doEvent(entity, gameManager, map);
                return true;
            }

            return false;
        }
    }
}
