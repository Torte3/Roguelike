using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities
{
    public class AStar
    {
        private HashSet<Vector2Int> _closeHash;
        private Dictionary<Vector2Int, AStarNode> _map;
        private HashSet<Vector2Int> _openHash;
        private Func<Vector2Int, Direction8, float> _canMove;

        public AStar(Func<Vector2Int, Direction8, float> canMove)
        {
            SetMap(canMove);
        }

        private void SetMap(Func<Vector2Int, Direction8, float> canMove)
        {
            _canMove = canMove;
            _openHash = new HashSet<Vector2Int>();
            _closeHash = new HashSet<Vector2Int>();
            _map = new Dictionary<Vector2Int, AStarNode>();
        }

        public List<Vector2Int> Calc(Vector2Int start, Vector2Int goal)
        {
            _map.Add(start, new AStarNode(start, goal));

            _openHash.Clear();
            _closeHash.Clear();

            var current = start;
            _openHash.Add(current);
            _map[current].Open(null, 0);
            var count = 1000;
            while (count-- > 0)
            {
                if (_openHash.Count <= 0)
                    return _map.Values.OrderBy(p => p.ECost).First().ToList();

                current = _openHash.OrderBy(p => _map[p].Score)
                    .First();

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

                _openHash.Remove(current);
                _closeHash.Add(current);

                OpenAround(current, goal);
            }

            return _map[current].ToList();
        }

        private void OpenAround(Vector2Int current, Vector2Int goal)
        {
            foreach (var direction in DirectionMethods.AllDirections)
            {
                var pos = current + direction.Vector();
                var cost = _canMove(current, direction);
                if (!_map.ContainsKey(pos))
                    if (cost < float.PositiveInfinity)
                        _map.Add(pos, new AStarNode(pos, goal));
                    else
                        continue;

                if (_openHash.Contains(pos))
                {
                    continue;
                }

                if (_closeHash.Contains(pos))
                {
                    continue;
                }

                _openHash.Add(pos);
                _map[pos].Open(_map[current], cost);
            }
        }
    }
}