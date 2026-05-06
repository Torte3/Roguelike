using System.Collections.Generic;
using System.Linq;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Area
{
    public class CircleArea : INotDirectionalArea
    {
        public bool ContainsSelf;
        public bool CanIgnoreWalls;
        [MinValue(1)] public int Radius;

        public CircleArea(int radius, bool containsSelf, bool canIgnoreWalls)
        {
            Radius = radius;
            ContainsSelf = containsSelf;
            CanIgnoreWalls = canIgnoreWalls;
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, IMap map)
        {
            if (CanIgnoreWalls || Radius <= 1)
                return EnumerableExtension.CircleRange(position, Radius + 0.5f)
                    .Where(p => ContainsSelf || p != position);
            return map.ComputeCircle(position => !map.At(position).IsBlank(EntityLayer.Middle), position, Radius + 0.5f)
                .Where(p => ContainsSelf || p != position);
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction, IMap map)
        {
            return Get(position, map);
        }

        public float EvaluateArea()
        {
            return CommonSenseParameters.CircleAreaEvaluate(CanIgnoreWalls, Radius);
        }

        public string Info()
        {
            var info = $"周囲{ItemDescriptionRichText.RichSpatial(Radius)}マス";
            if (ContainsSelf) info += "(中心含む)";
            if (CanIgnoreWalls) info += "(壁無視)";
            return info;
        }
    }
}