using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Map;

namespace Domain.Service.Action
{
    internal record PickUpItem() : IAction
    {
        public bool Doable(IActor actor, IMap map)
        {
            var Item = map.Items.At(actor.Entity.CurrentPosition).FirstOrDefault()?.Item;
            if (Item == null)
                return false;
            if (!actor.Inventory.CanAddToEmpty())
                return false;
            return !actor.Status.IsFlagStat(FlagStatType.CannotAct);
        }

        public UniTask Do(IActor actor, IMap map, IInput input)
        {
            actor.PickUpItem(map);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActor actor, IMap map)
        {
            return 0;
        }

        public string Info()
        {
            return $"PickUpItem";
        }
    }
}