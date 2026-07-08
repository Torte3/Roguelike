using System;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Service.ItemEffect
{
    [Serializable]
    public class UpgradeItem : IItemEffect
    {
        public bool CanApplyTo(IItem item, IMap map)
        {
            return item.CanUpgrade();
        }

        public void Apply(IItem item, IEntity itemHolder, IMap map)
        {
            item.Upgrade(itemHolder, map);
        }

        public float EvaluatePrice()
        {
            return 1000;
        }

        public string Description()
        {
            return "強化";
        }
    }
}