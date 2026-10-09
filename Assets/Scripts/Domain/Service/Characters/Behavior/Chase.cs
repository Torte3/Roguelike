#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;
using Unity.Logging;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal static class Chase
    {
        public static IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, Vector2Int targetPosition,
            IMap map)
        {
            var route = character.RouteTo(targetPosition, map);
            if (route.HasNoStep)
            {
                Log.Debug("[Think]Already reached the target position");
                return Enumerable.Empty<Move>();
            }

            var direction = DirectionMethods.FromVector(route.FirstStep - route.Start);

            var move = new Move(direction!.Value, 0.01f);
            var swap = new Swap(direction!.Value, 0.01f);
            if (move.Doable(character, map))
            {
                return new List<Move> { move };
            }

            if (swap.Doable(character, map) &&
                map.GetCharacterAt(route.FirstStep)?.AcceptsSwapFrom(character.Entity.CurrentPosition, map) == true)
            {
                return new List<Swap> { swap };
            }

            Log.Debug($"[Think]Move to {direction} is not doable");
            return Enumerable.Empty<Move>();
        }

        public static Direction8? NextStep(IHasBehavior character, Vector2Int targetPosition, IMap map)
        {
            var route = character.RouteTo(targetPosition, map);
            return route.HasNoStep ? null : DirectionMethods.FromVector(route.FirstStep - route.Start);
        }
    }
}