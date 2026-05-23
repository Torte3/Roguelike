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
    internal record UseItem(IItem Item, Direction8 Direction) : IAction
    {
        public bool Doable(IActor actor, IMap map)
        {
            if (actor.Status.IsFlagStat(FlagStatType.CannotAct))
            {
                return false;
            }

            if (Item.IsEquipped.IsSome() && !actor.Inventory.Contains(Item))
            {
                return false;
            }

            if (!actor.Inventory.CanRemove(Item)
                && map.Items.At(actor.Entity.CurrentPosition).FirstOrDefault()?.Item != Item)
            {
                return false;
            }

            if (!Item.IsInfoIdentified(map.Player) && Item.HasActivatableSkillWhenUsed)
            {
                return true;
            }

            return Item.CanAttemptUse;
        }

        public async UniTask Do(IActor actor, IMap map, IInput input)
        {
            await actor.UseItem(Item, Direction, map);
        }

        public float Evaluate(IActor actor, IMap map)
        {
            return Item.EvaluateWhenUsed(actor, actor.Entity.CurrentPosition, Direction, map);
        }

        public string Info()
        {
            return $"UseItem: Item:{Item.DebugInfo()}, Direction:{Direction}";
        }
    }
}