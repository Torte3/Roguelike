#nullable enable
using Domain.Model.Entity;
using Domain.Model.Map;

namespace Domain.Model.Item
{
    public interface IInventoryEffect
    {
        public void Apply(IStorage storage, IEntity itemHolder, IMap map);
        public float EvaluatePrice();
        public string Description();
    }
}