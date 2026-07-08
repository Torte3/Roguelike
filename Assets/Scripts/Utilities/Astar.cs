using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities
{
    public class AStar
    {
        private readonly struct OpenEntry
        {
            public readonly float Score;
            public readonly float Estimate;
            public readonly int Order;
            public readonly Vector2Int Position;

            public OpenEntry(float score, float estimate, int order, Vector2Int position)
            {
                Score = score;
                Estimate = estimate;
                Order = order;
                Position = position;
            }
        }

        private readonly HashSet<Vector2Int> _closeHash = new();
        private readonly Dictionary<Vector2Int, AStarNode> _map = new();
        private readonly HashSet<Vector2Int> _openHash = new();
        private readonly List<OpenEntry> _openHeap = new();
        private int _openOrder;
        private readonly Func<Vector2Int, Direction8, float> _canMove;

        private AStar(Func<Vector2Int, Direction8, float> canMove)
        {
            _canMove = canMove;
        }

        public static Route FindRoute(Func<Vector2Int, Direction8, float> canMove, Vector2Int start, Vector2Int goal)
        {
            return new AStar(canMove).Calc(start, goal);
        }

        private Route Calc(Vector2Int start, Vector2Int goal)
        {
            _map.Add(start, new AStarNode(start, goal));

            var current = start;
            _map[current].Open(null, 0);
            AddOpen(current);
            var count = 1000;
            while (count-- > 0)
            {
                if (_openHash.Count <= 0)
                    return new Route(_map.Values.OrderBy(p => p.ECost).First().ToList());

                current = PopOpen();

                if (_map[current].ECost <= 1)
                {
                    var direction = DirectionMethods.FromVectorStrict(goal - current);
                    if (direction != null && _canMove(current, direction.Value) < float.PositiveInfinity)
                    {
                        if (_map[current].ECost <= 0)
                            break;
                    }
                    else
                        break;
                }

                _closeHash.Add(current);

                OpenAround(current, goal);
            }

            return new Route(_map[current].ToList());
        }

        private void OpenAround(Vector2Int current, Vector2Int goal)
        {
            foreach (var direction in DirectionMethods.AllDirections)
            {
                var pos = current + direction.Vector();
                var cost = _canMove(current, direction);
                if (cost >= float.PositiveInfinity)
                    continue;

                if (!_map.ContainsKey(pos))
                    _map.Add(pos, new AStarNode(pos, goal));

                if (_closeHash.Contains(pos))
                    continue;

                if (_openHash.Contains(pos) && _map[current].MoveTotalCost + cost >= _map[pos].MoveTotalCost)
                    continue;

                _map[pos].Open(_map[current], cost);
                AddOpen(pos);
            }
        }

        private void AddOpen(Vector2Int position)
        {
            _openHash.Add(position);
            _openHeap.Add(new OpenEntry(_map[position].Score, _map[position].ECost, _openOrder++, position));
            var child = _openHeap.Count - 1;
            while (child > 0)
            {
                var parent = (child - 1) / 2;
                if (!IsBefore(_openHeap[child], _openHeap[parent]))
                    break;
                (_openHeap[child], _openHeap[parent]) = (_openHeap[parent], _openHeap[child]);
                child = parent;
            }
        }

        private Vector2Int PopOpen()
        {
            while (true)
            {
                var entry = PopHeap();
                if (_openHash.Contains(entry.Position) && entry.Score == _map[entry.Position].Score)
                {
                    _openHash.Remove(entry.Position);
                    return entry.Position;
                }
            }
        }

        private OpenEntry PopHeap()
        {
            var top = _openHeap[0];
            var last = _openHeap.Count - 1;
            _openHeap[0] = _openHeap[last];
            _openHeap.RemoveAt(last);
            var parent = 0;
            while (true)
            {
                var left = parent * 2 + 1;
                if (left >= _openHeap.Count)
                    break;
                var right = left + 1;
                var smaller = right < _openHeap.Count && IsBefore(_openHeap[right], _openHeap[left]) ? right : left;
                if (!IsBefore(_openHeap[smaller], _openHeap[parent]))
                    break;
                (_openHeap[smaller], _openHeap[parent]) = (_openHeap[parent], _openHeap[smaller]);
                parent = smaller;
            }

            return top;
        }

        private static bool IsBefore(OpenEntry a, OpenEntry b)
        {
            if (a.Score != b.Score)
                return a.Score < b.Score;
            if (a.Estimate != b.Estimate)
                return a.Estimate < b.Estimate;
            return a.Order < b.Order;
        }
    }
}