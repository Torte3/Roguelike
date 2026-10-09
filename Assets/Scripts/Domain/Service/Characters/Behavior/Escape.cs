#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal static class Escape
    {
        public static IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap map)
        {
            var relativePosition = character.Entity.CurrentPosition - targetPosition;
            var directions = DirectionMethods.NearDirectionsFromVector(relativePosition);
            IEnumerable<Move> moves;
            if (directions != null)
            {
                moves = new List<Move>
                    { new(directions[0], 0.02f), new(directions[1], 0.005f), new(directions[2], 0.005f) };
            }
            else
            {
                moves = DirectionMethods.AllDirections.Select(direction => new Move(direction, 0.01f));
            }

            return moves.Where(move => move.Doable(character, map));
        }

        public static Direction8? NextStep(IHasBehavior character, Vector2Int targetPosition)
        {
            return DirectionMethods.NearDirectionsFromVector(character.Entity.CurrentPosition - targetPosition)?[0];
        }
    }
}