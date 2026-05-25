using Cysharp.Threading.Tasks;
using Domain.Model.Map;
using Utilities.Stats;

namespace Domain.Model
{
    public interface IScheduledEvent
    {
        public ResourceData WaitTurnData { get; }
        public void UpdateTurn();
        public bool CanExecuteEvent();
        public UniTask<bool> DoEvent(IGameManager gameManager, IMap map);
    }
}