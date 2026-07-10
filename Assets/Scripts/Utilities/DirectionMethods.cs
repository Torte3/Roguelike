#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Utilities
{
    public static class DirectionMethods
    {
        public static readonly IEnumerable<Direction8> AllDirections =
            (IEnumerable<Direction8>)Enum.GetValues(typeof(Direction8));

        public static Angle Angle(this Direction8 direction)
        {
            return direction switch
            {
                Direction8.Up => new Angle(90),
                Direction8.UpRight => new Angle(45),
                Direction8.Right => new Angle(0),
                Direction8.DownRight => new Angle(315),
                Direction8.Down => new Angle(270),
                Direction8.DownLeft => new Angle(225),
                Direction8.Left => new Angle(180),
                Direction8.UpLeft => new Angle(135),
                _ => throw new InvalidEnumArgumentException()
            };
        }

        private static Direction8 FromAngle(Angle angle)
        {
            return angle.Value switch
            {
                90 => Direction8.Up,
                45 => Direction8.UpRight,
                0 => Direction8.Right,
                315 => Direction8.DownRight,
                270 => Direction8.Down,
                225 => Direction8.DownLeft,
                180 => Direction8.Left,
                135 => Direction8.UpLeft,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public static Vector2Int Vector(this Direction8 direction)
        {
            return direction switch
            {
                Direction8.Up => new Vector2Int(0, 1),
                Direction8.UpRight => new Vector2Int(1, 1),
                Direction8.Right => new Vector2Int(1, 0),
                Direction8.DownRight => new Vector2Int(1, -1),
                Direction8.Down => new Vector2Int(0, -1),
                Direction8.DownLeft => new Vector2Int(-1, -1),
                Direction8.Left => new Vector2Int(-1, 0),
                Direction8.UpLeft => new Vector2Int(-1, 1),
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public static Direction8? FromVector(Vector2 vector)
        {
            if (Mathf.Approximately(vector.x, 0) && Mathf.Approximately(vector.y, 0))
            {
                return null;
            }

            return vector switch
            {
                { x: 0, y: > 0 } => Direction8.Up,
                { x: > 0, y: > 0 } => Direction8.UpRight,
                { x: > 0, y: 0 } => Direction8.Right,
                { x: > 0, y: < 0 } => Direction8.DownRight,
                { x: 0, y: < 0 } => Direction8.Down,
                { x: < 0, y: < 0 } => Direction8.DownLeft,
                { x: < 0, y: 0 } => Direction8.Left,
                { x: < 0, y: > 0 } => Direction8.UpLeft,
                _ => throw new ArgumentException()
            };
        }

        public static Direction8? FromVectorStrict(Vector2 vector)
        {
            if (Mathf.Approximately(vector.x, 0) && Mathf.Approximately(vector.y, 0))
            {
                return null;
            }

            if (Mathf.Approximately(vector.x, 0) || Mathf.Approximately(vector.y, 0) ||
                Mathf.Approximately(Mathf.Abs(vector.x), Mathf.Abs(vector.y)))
            {
                return vector switch
                {
                    { x: 0, y: > 0 } => Direction8.Up,
                    { x: > 0, y: > 0 } => Direction8.UpRight,
                    { x: > 0, y: 0 } => Direction8.Right,
                    { x: > 0, y: < 0 } => Direction8.DownRight,
                    { x: 0, y: < 0 } => Direction8.Down,
                    { x: < 0, y: < 0 } => Direction8.DownLeft,
                    { x: < 0, y: 0 } => Direction8.Left,
                    { x: < 0, y: > 0 } => Direction8.UpLeft,
                    _ => throw new ArgumentException($"Value {vector} does not fall within the expected range.")
                };
            }

            return null;
        }

        public static Angle AngleTo(this Direction8 direction, Direction8 otherDirection)
        {
            return otherDirection.Angle() - direction.Angle();
        }

        public static Direction8? NearestDirectionFromVector(Vector2 vector)
        {
            if (Mathf.Approximately(vector.x, 0) && Mathf.Approximately(vector.y, 0))
            {
                return null;
            }

            return new Angle(vector).Value switch
            {
                < 0 + 45 / 2 => Direction8.Right,
                < 45 + 45 / 2 => Direction8.UpRight,
                < 90 + 45 / 2 => Direction8.Up,
                < 135 + 45 / 2 => Direction8.UpLeft,
                < 180 + 45 / 2 => Direction8.Left,
                < 225 + 45 / 2 => Direction8.DownLeft,
                < 270 + 45 / 2 => Direction8.Down,
                < 315 + 45 / 2 => Direction8.DownRight,
                < 360 + 45 / 2 => Direction8.Right,
                _ => throw new ArgumentException()
            };
        }

        public static List<Direction8>? NearDirectionsFromVector(Vector2 vector)
        {
            if (Mathf.Approximately(vector.x, 0) && Mathf.Approximately(vector.y, 0))
            {
                return null;
            }

            return new Angle(vector).Value switch
            {
                < 0 + 45 / 2 => new List<Direction8> { Direction8.Right, Direction8.UpRight, Direction8.Up },
                < 45 => new List<Direction8> { Direction8.UpRight, Direction8.Right, Direction8.Up },
                < 45 + 45 / 2 => new List<Direction8> { Direction8.UpRight, Direction8.Up, Direction8.Right },
                < 90 => new List<Direction8> { Direction8.Up, Direction8.UpRight, Direction8.Right },
                < 90 + 45 / 2 => new List<Direction8> { Direction8.Up, Direction8.UpLeft, Direction8.Left },
                < 135 => new List<Direction8> { Direction8.UpLeft, Direction8.Up, Direction8.Left },
                < 135 + 45 / 2 => new List<Direction8> { Direction8.UpLeft, Direction8.Left, Direction8.Up },
                < 180 => new List<Direction8> { Direction8.Left, Direction8.UpLeft, Direction8.Up },
                < 180 + 45 / 2 => new List<Direction8> { Direction8.Left, Direction8.DownLeft, Direction8.Down },
                < 225 => new List<Direction8> { Direction8.DownLeft, Direction8.Left, Direction8.Down },
                < 225 + 45 / 2 => new List<Direction8> { Direction8.DownLeft, Direction8.Down, Direction8.Left },
                < 270 => new List<Direction8> { Direction8.Down, Direction8.DownLeft, Direction8.Left },
                < 270 + 45 / 2 => new List<Direction8> { Direction8.Down, Direction8.DownRight, Direction8.Right },
                < 315 => new List<Direction8> { Direction8.DownRight, Direction8.Down, Direction8.Right },
                < 315 + 45 / 2 => new List<Direction8> { Direction8.DownRight, Direction8.Right, Direction8.Down },
                < 360 => new List<Direction8> { Direction8.Right, Direction8.DownRight, Direction8.Down },
                _ => throw new ArgumentException()
            };
        }

        public static bool IsDiagonal(this Direction8 direction)
        {
            return direction switch
            {
                Direction8.Up => false,
                Direction8.UpRight => true,
                Direction8.Right => false,
                Direction8.DownRight => true,
                Direction8.Down => false,
                Direction8.DownLeft => true,
                Direction8.Left => false,
                Direction8.UpLeft => true,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public static Direction8 RotateClockwise(this Direction8 direction, Angle angle)
        {
            return FromAngle(direction.Angle().RotateClockwise(angle.Value));
        }

        public static Direction8 RotateAntiClockwise(this Direction8 direction, Angle angle)
        {
            return FromAngle(direction.Angle().RotateAntiClockwise(angle.Value));
        }

        public static Direction8 Rotate45Clockwise(this Direction8 direction)
        {
            return direction switch
            {
                Direction8.Up => Direction8.UpRight,
                Direction8.UpRight => Direction8.Right,
                Direction8.Right => Direction8.DownRight,
                Direction8.DownRight => Direction8.Down,
                Direction8.Down => Direction8.DownLeft,
                Direction8.DownLeft => Direction8.Left,
                Direction8.Left => Direction8.UpLeft,
                Direction8.UpLeft => Direction8.Up,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public static Direction8 Rotate90Clockwise(this Direction8 direction)
        {
            return direction switch
            {
                Direction8.Up => Direction8.Right,
                Direction8.UpRight => Direction8.DownRight,
                Direction8.Right => Direction8.Down,
                Direction8.DownRight => Direction8.DownLeft,
                Direction8.Down => Direction8.Left,
                Direction8.DownLeft => Direction8.UpLeft,
                Direction8.Left => Direction8.Up,
                Direction8.UpLeft => Direction8.UpRight,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public static Direction8 Rotate45AntiClockwise(this Direction8 direction)
        {
            return direction switch
            {
                Direction8.Up => Direction8.UpLeft,
                Direction8.UpRight => Direction8.Up,
                Direction8.Right => Direction8.UpRight,
                Direction8.DownRight => Direction8.Right,
                Direction8.Down => Direction8.DownRight,
                Direction8.DownLeft => Direction8.Down,
                Direction8.Left => Direction8.DownLeft,
                Direction8.UpLeft => Direction8.Left,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public static Direction8 Rotate90AntiClockwise(this Direction8 direction)
        {
            return direction switch
            {
                Direction8.Up => Direction8.Left,
                Direction8.UpRight => Direction8.UpLeft,
                Direction8.Right => Direction8.Up,
                Direction8.DownRight => Direction8.UpRight,
                Direction8.Down => Direction8.Right,
                Direction8.DownLeft => Direction8.DownRight,
                Direction8.Left => Direction8.Down,
                Direction8.UpLeft => Direction8.DownLeft,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public static Direction8 Reverse(this Direction8 direction)
        {
            return direction switch
            {
                Direction8.Up => Direction8.Down,
                Direction8.UpRight => Direction8.DownLeft,
                Direction8.Right => Direction8.Left,
                Direction8.DownRight => Direction8.UpLeft,
                Direction8.Down => Direction8.Up,
                Direction8.DownLeft => Direction8.UpRight,
                Direction8.Left => Direction8.Right,
                Direction8.UpLeft => Direction8.DownRight,
                _ => throw new InvalidEnumArgumentException()
            };
        }
    }
}