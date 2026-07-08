using System.Text;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Service.Action
{
    internal record DropItem(IItem Item) : IAction
    {
        protected virtual bool PrintMembers(StringBuilder builder)
        {
            builder.Append($"Item = {Item.DebugInfo()}");
            return true;
        }

        public bool Doable(IActor actor, IMap map)
        {
            if (!actor.Inventory.CanRemove(Item))
                return false;
            if (Item.IsDiscardBlocked)
                return false;
            return !actor.Status.IsFlagStat(FlagStatType.CannotAct);
        }

        public UniTask Do(IActor actor, IMap map)
        {
            actor.DropItem(Item, map);
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActor actor, IMap map)
        {
            return 0;
        }
    }
}