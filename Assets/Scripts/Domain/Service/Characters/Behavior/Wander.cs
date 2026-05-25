using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Service.Action;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal static class Wander
    {
        public static IEnumerable<IAction> GenerateMoveActionsDoable(IHasBehavior character, IMap map)
        {
            var directions = new Dictionary<Direction8, float>();
            var facingDirection = character.CurrentDirection;
            if (character.CanMove(facingDirection, false, false, map))
            {
                directions.Add(facingDirection, 0.1f);
                if (!map.At(character.Entity.CurrentPosition +
                            facingDirection.RotateClockwise(new Angle(135)).Vector()).IsWalkableOnMap()
                    && character.CanMove(facingDirection.RotateClockwise(new Angle(90)), false, false, map))
                {
                    directions.Add(facingDirection.RotateClockwise(new Angle(90)), 0.1f);
                }

                if (!map.At(character.Entity.CurrentPosition +
                            facingDirection.RotateAntiClockwise(new Angle(135)).Vector()).IsWalkableOnMap()
                    && character.CanMove(facingDirection.RotateAntiClockwise(new Angle(90)), false, false, map))
                {
                    directions.Add(facingDirection.RotateAntiClockwise(new Angle(90)), 0.1f);
                }
            }
            else
            {
                if (character.CanMove(facingDirection.Rotate90Clockwise(), false, false, map))
                {
                    directions.Add(facingDirection.Rotate90Clockwise(), 0.1f);
                }

                if (character.CanMove(facingDirection.Rotate90AntiClockwise(), false, false, map))
                {
                    directions.Add(facingDirection.Rotate90AntiClockwise(), 0.1f);
                }

                if (character.CanMove(facingDirection.Rotate45Clockwise(), false, false, map))
                {
                    directions.Add(facingDirection.Rotate45Clockwise(), 0.05f);
                }

                if (character.CanMove(facingDirection.Rotate45AntiClockwise(), false, false, map))
                {
                    directions.Add(facingDirection.Rotate45AntiClockwise(), 0.05f);
                }

                if (character.CanMove(facingDirection.Reverse().Rotate45Clockwise(), false, false, map))
                {
                    directions.Add(facingDirection.Reverse().Rotate45Clockwise(), 0.02f);
                }

                if (character.CanMove(facingDirection.Reverse().Rotate45AntiClockwise(), false, false, map))
                {
                    directions.Add(facingDirection.Reverse().Rotate45AntiClockwise(), 0.02f);
                }

                if (character.CanMove(facingDirection.Reverse(), false, false, map))
                {
                    directions.Add(facingDirection.Reverse(), 0.03f);
                }
            }

            if (directions.Any())
            {
                return directions.Select(direction => new Move(direction.Key, direction.Value));
            }

            if (character.CanMove(facingDirection, map))
            {
                return new[] { new Move(facingDirection, 0.05f) };
            }

            return DirectionMethods.AllDirections.Where(direction => character.CanMove(direction, map))
                .Select(direction => new Move(direction, 0.02f));
        }
    }
}