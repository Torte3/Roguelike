using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class RandomWalk : IMovement
    {
        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition, IMap map)
        {
            return DirectionMethods.AllDirections.Where(direction => character.CanMove(direction, map))
                .Select(direction => new Move(direction));
        }

        public Direction8? NextStep(IHasBehavior character, Vector2Int targetPosition, IMap map) => null;
    }
}
