using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Model.Setting;
using Domain.Service.Action;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal sealed class IntelligentDashController
    {
        private Direction8? _lastMoveDirection;

        public async UniTask Wait(IHasBehavior character, IMap map)
        {
            if (character.CanMove(character.CurrentDirection, map) &&
                character.CanMove(character.CurrentDirection.Reverse(), map) &&
                (
                    (character.CanMove(character.CurrentDirection.Rotate90Clockwise(), map) &&
                     !character.CanMove(character.CurrentDirection.Reverse().Rotate45AntiClockwise(), map))
                    || (character.CanMove(character.CurrentDirection.Rotate90AntiClockwise(), map) &&
                        !character.CanMove(character.CurrentDirection.Reverse().Rotate45Clockwise(), map))
                )
               )
                await UniTask.Delay(Settings.GlobalSettings.DashPauseMilliseconds.CurrentValue);
        }

        public Move Filter(Move move, IHasBehavior character, bool isStarted, IMap map, IInput input)
        {
            move = MoveFilter(move, character, map);
            move = DashFilter(move, character, isStarted, map, input);
            _lastMoveDirection = move.Direction;
            return move;
        }

        private Move MoveFilter(Move move, IHasBehavior character, IMap map)
        {
            if (!character.CanMove(move.Direction, map) && move.Direction.IsDiagonal())
            {
                var directionRotateClockwise = move.Direction.Rotate45Clockwise();
                var canMoveDirectionRotateClockwise = character.CanMove(directionRotateClockwise, map);
                var directionRotateAntiClockwise = move.Direction.Rotate45AntiClockwise();
                var canMoveDirectionRotateAntiClockwise = character.CanMove(directionRotateAntiClockwise, map);
                if (canMoveDirectionRotateClockwise && !canMoveDirectionRotateAntiClockwise)
                    return new Move(directionRotateClockwise);
                if (!canMoveDirectionRotateClockwise && canMoveDirectionRotateAntiClockwise)
                    return new Move(directionRotateAntiClockwise);
            }

            return move;
        }

        private Move DashFilter(Move move, IHasBehavior character, bool isStarted, IMap map, IInput input)
        {
            if (!input.IsDash() || isStarted)
                return move;
            if (InPathway(character, map))
            {
                if (_lastMoveDirection.HasValue && character.CanMove(_lastMoveDirection.Value, map))
                    return new Move(_lastMoveDirection.Value);
                var canMoveDirections = DirectionMethods.AllDirections
                    .Where(direction => !direction.IsDiagonal())
                    .Where(direction => character.CanMove(direction, map))
                    .Where(direction => direction != _lastMoveDirection?.Reverse());
                if (canMoveDirections.Count() == 1)
                    return new Move(canMoveDirections.First());
            }

            return move;
        }

        private bool InPathway(IHasBehavior character, IMap map)
        {
            var canMoveDirections = DirectionMethods.AllDirections
                .Where(direction => character.CanMoveIgnoreEntity(direction, map))
                .ToList();

            var isStraightPathClear = canMoveDirections
                .Where(direction => !direction.IsDiagonal())
                .Count() == 2;

            var noValidDiagonalPath = !canMoveDirections
                .Where(direction => direction.IsDiagonal())
                .Any(direction => canMoveDirections.Contains(direction.Rotate45Clockwise()) &&
                                  canMoveDirections.Contains(direction.Rotate45AntiClockwise()));

            return isStraightPathClear && noValidDiagonalPath;
        }
    }
}