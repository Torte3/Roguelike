using System.Collections.Generic;
using System.Linq;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Area
{
    public class LineArea : IArea
    {
        public bool ContainsSelf;
        public bool CanIgnoreWalls;
        [MinValue(1)] public int Length;

        public LineArea(int length, bool containsSelf, bool canIgnoreWalls)
        {
            Length = length;
            CanIgnoreWalls = canIgnoreWalls;
            ContainsSelf = containsSelf;
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction, IMap map)
        {
            var area = (ContainsSelf ? Enumerable.Range(0, Length + 1) : Enumerable.Range(1, Length))
                .Select(i => position + direction.Vector() * i);
            foreach (var pos in area)
            {
                yield return pos;
                if (!CanIgnoreWalls && !map.At(pos).IsBlank(EntityLayer.Middle))
                    break;
            }
        }

        public float EvaluateArea()
        {
            if (CanIgnoreWalls)
                return Mathf.Sqrt(Length) * 2 - 1;
            return Mathf.Sqrt(Length);
        }

        public string Info()
        {
            var info = $"前{ItemDescriptionRichText.RichSpatialCells(Length)}";
            if (ContainsSelf) info += "(原点含む)";
            if (CanIgnoreWalls) info += "(壁無視)";
            return info;
        }
    }
}