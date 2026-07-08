using Domain.Model.Character;
using Domain.Model.Character.Data;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Map;
using Utilities;

namespace Domain.Model.Effect
{
    public interface ITargetOfEffect : IHasLabel, IHasStatus, IHasInventory, IEntity
    {
        public IVisionRange VisionRange { get; }
        public void AddCondition(Id<IEntity> actor, ConditionTemplate condition);
        public void ClearCondition();
        public void ClearKnownItems(IMap map);
        public void ClearAffiliation(IMap map);
        public void ListenToAlert(Location location);
        public void ForceDropItem(int index, IMap map);
    }
}