#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Service.Events
{
    internal class CharacterEvent : ICharacterEvent
    {
        private readonly Func<ICharacter, bool> _canExecuteEvent;
        private readonly Func<ICharacter, IGameManager, IMap, UniTask> _doEvent;

        public CharacterEvent(Func<ICharacter, bool> canExecuteEvent,
            Func<ICharacter, IGameManager, IMap, UniTask> doEvent)
        {
            _canExecuteEvent = canExecuteEvent;
            _doEvent = doEvent;
        }

        public bool CanExecuteEvent(ICharacter character)
        {
            return _canExecuteEvent(character);
        }

        public async UniTask<bool> DoEvent(ICharacter character, IGameManager gameManager, IMap map)
        {
            if (_canExecuteEvent(character))
            {
                await _doEvent(character, gameManager, map);
                return true;
            }

            return false;
        }
    }
}