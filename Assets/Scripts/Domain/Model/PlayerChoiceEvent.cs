#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model
{
    public class PlayerChoiceEvent
    {
        public string ChoiceText { get; init; }
        private readonly Func<IPlayer, IMap, bool> _canExecuteEvent;
        private readonly Func<IGameManager, IMap, UniTask> _doEvent;

        public PlayerChoiceEvent(string choiceText, Func<IPlayer, IMap, bool> canExecuteEvent,
            Func<IGameManager, IMap, UniTask> doEvent)
        {
            ChoiceText = choiceText;
            _canExecuteEvent = canExecuteEvent;
            _doEvent = doEvent;
        }

        public bool CanExecuteEvent(IPlayer player, IMap map)
        {
            return _canExecuteEvent(player, map);
        }

        public async UniTask<bool> DoEvent(IPlayer player, IGameManager gameManager, IMap map)
        {
            if (CanExecuteEvent(player, map))
            {
                await _doEvent(gameManager, map);
                return true;
            }

            return false;
        }
    }
}