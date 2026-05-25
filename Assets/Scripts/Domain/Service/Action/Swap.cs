using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Map;
using Utilities;

namespace Domain.Service.Action
{
    internal record Swap(Direction8 Direction, float Score = 0) : IAction
    {
        public bool Doable(IActor actor, IMap map)
        {
            return !actor.Status.IsFlagStat(FlagStatType.CannotAct) &&
                   !actor.Status.IsFlagStat(FlagStatType.CannotMove) && actor.CanSwap(Direction, map);
        }

        public UniTask Do(IActor actor, IMap map, IInput input)
        {
            var target = map.Characters.At(actor.Entity.CurrentPosition + Direction.Vector()).FirstOrDefault();
            if (target == null)
                throw new InvalidOperationException("target is null");
            actor.Entity.IsVisualOnly.Value = true;
            actor.Move(Direction, input).Forget();
            target.ForceMove(Direction.Reverse(), input).Forget();
            actor.Entity.IsVisualOnly.Value = false;
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActor actor, IMap map)
        {
            return Score;
        }

        public string Info()
        {
            return $"Swap: Direction:{Direction}";
        }
    }
}