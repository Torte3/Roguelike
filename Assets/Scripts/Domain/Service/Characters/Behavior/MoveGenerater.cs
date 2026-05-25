#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Service.Characters.Behavior
{
    internal static class MoveGenerater
    {
        public static IEnumerable<IAction> GenerateDoableMovesWhenUndiscoveringTarget(MoveTypeWhenUndiscoveringTarget moveType, IHasBehavior character, IMap map)
        {
            return moveType switch
            {
                MoveTypeWhenUndiscoveringTarget.Wander => Wander.GenerateMoveActionsDoable(character, map),
                MoveTypeWhenUndiscoveringTarget.NoMove => NoMove.GenerateMoveActionsDoable(character, map),
                _ => throw new ArgumentException($"Invalid move type: {moveType}")
            };
        }

        public static IEnumerable<IAction> GenerateDoableMovesWhenDiscoveringTarget(MoveTypeWhenDiscoveringTarget moveType, IHasBehavior character, Vector2Int targetPosition,
            IMap map)
        {
            return moveType switch
            {
                MoveTypeWhenDiscoveringTarget.Chase => Chase.GenerateMoveActionsDoable(character, targetPosition, map),
                MoveTypeWhenDiscoveringTarget.Escape => Escape.GenerateMoveActionsDoable(character, targetPosition, map),
                MoveTypeWhenDiscoveringTarget.Wander => Wander.GenerateMoveActionsDoable(character, map),
                MoveTypeWhenDiscoveringTarget.NoMove => NoMove.GenerateMoveActionsDoable(character, map),
                _ => throw new ArgumentException($"Invalid move type: {moveType}")
            };
        }
    }
}