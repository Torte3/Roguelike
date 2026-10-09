using System.Linq;
using Domain.Model.Map;

namespace Domain.Model.Entity
{
    public interface IPlayerEventEntity : IHasPlayerEvent, IEntity
    {
        bool IEntity.AppearsInteractable(IMap map) => Events.Any(e => e.CanExecuteEvent(map.Player, map));
    }
}
