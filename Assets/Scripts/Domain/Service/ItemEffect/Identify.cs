using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Service.ItemEffect
{
    public class Identify : IItemEffect
    {
        public bool CanApplyTo(IItem item, IMap map)
        {
            return !map.Player.Character.IsKnownItem(item) || !map.Player.Character.IsCurseKnown(item);
        }

        public void Apply(IItem item, IEntity itemHolder, IMap map)
        {
            map.Player.Character.KnowItem(item, true);
            map.Player.Character.KnowCurse(item, true);
        }

        public float EvaluatePrice()
        {
            return 100;
        }

        public string Description()
        {
            return "識別";
        }
    }
}