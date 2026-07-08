#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal interface IMovement
    {
        public IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition, IMap map);
        public Direction8? NextStep(IHasBehavior character, Vector2Int targetPosition, IMap map);
    }
}
