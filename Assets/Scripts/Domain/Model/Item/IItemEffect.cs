#nullable enable
using Domain.Model.Entity;
using Domain.Model.Map;

namespace Domain.Model.Item
{
    public interface IItemEffect
    {
        public bool CanApplyTo(IItem item, IMap map);
        public void Apply(IItem item, IEntity itemHolder, IMap map);
        public float EvaluatePrice();
        public string Description();
    }
}