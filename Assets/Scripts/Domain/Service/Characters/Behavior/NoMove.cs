using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class NoMove : IMovement
    {
        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition, IMap map)
        {
            return new List<IAction> { new DoNothing() };
        }

        public Direction8? NextStep(IHasBehavior character, Vector2Int targetPosition, IMap map) => null;
    }
}
