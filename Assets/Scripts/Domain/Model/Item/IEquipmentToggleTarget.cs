#nullable enable
using Domain.Model.Effect;
using Domain.Model.Map;

namespace Domain.Model.Item
{
    public interface IEquipmentToggleTarget
    {
        bool TryToggleEquipped(IActorOfEffect actor, IMap map);
        void ForceUnequip();
    }
}
