using Cysharp.Threading.Tasks;
using Domain.Model.Entity;
using Domain.Model.Map;

namespace Domain.Model
{
    public interface IEntityEvent
    {
        bool CanExecuteEvent(IEntity entity);

        UniTask<bool> DoEvent(IEntity entity, IGameManager gameManager, IMap map);
    }
}
