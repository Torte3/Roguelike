#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Model
{
    public interface IPlayerEvent
    {
        public string? ChoiceMessage { get; }
        public IReadOnlyList<PlayerChoiceEvent> Events { get; }
        public bool CanExecuteEvent(IPlayer player, IMap map);
        public UniTask<bool> DoEvent(IPlayer player, IGameManager gameManager, IMap map);
        public UniTask<IAction?> DoAction(IPlayer player, IGameManager gameManager, IMap map, IAction? swap);
    }
}