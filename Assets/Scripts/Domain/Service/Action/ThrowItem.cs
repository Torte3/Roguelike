#nullable enable
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Item;
using Domain.Model.Map;
using Utilities;

namespace Domain.Service.Action
{
    internal record ThrowItem(IItem Item, Direction8 Direction) : IAction
    {
        public bool Doable(IActor actor, IMap map)
        {
            if (actor.Status.IsFlagStat(FlagStatType.CannotAct))
            {
                return false;
            }

            if (!actor.Inventory.CanRemove(Item)
                && map.Items.At(actor.Entity.CurrentPosition).FirstOrDefault()?.Item != Item)
            {
                return false;
            }

            if (!Item.CanAttemptThrow)
                return false;

            return true;
        }

        public async UniTask Do(IActor actor, IMap map, IInput input)
        {
            await actor.ThrowItem(Item, Direction, map);
        }

        public float Evaluate(IActor actor, IMap map)
        {
            if (Item.RemainingUses.CurrentValue > 0)
                return actor.EvaluateThrow(Item, Direction, map) / Item.RemainingUses.CurrentValue;
            return 0;
        }

        public string Info()
        {
            return $"ThrowItem: Item:{Item.DebugInfo()}, Direction:{Direction}";
        }
    }
}