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
    public class FanArea : IArea
    {
        public bool ContainsSelf;
        public bool CanIgnoreWalls;
        [MinValue(1)] public int Radius;

        public FanArea(int radius, bool containsSelf, bool canIgnoreWalls)
        {
            Radius = radius;
            ContainsSelf = containsSelf;
            CanIgnoreWalls = canIgnoreWalls;
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction, IMap map)
        {
            var area = new List<Vector2Int>();

            switch (direction)
            {
                case Direction8.Up:
                case Direction8.Down:
                case Direction8.Left:
                case Direction8.Right:
                    var deltaVec = direction.Vector();
                    var perpVec = direction.Rotate90Clockwise().Vector();
                    for (var i = 0; i <= Radius; i++)
                    {
                        for (var j = -i; j <= i; j++)
                        {
                            if (i * i + j * j <= (Radius + 0.5f) * (Radius + 0.5f))
                            {
                                area.Add(position + i * deltaVec + j * perpVec);
                            }
                        }
                    }

                    break;

                case Direction8.UpLeft:
                case Direction8.UpRight:
                case Direction8.DownLeft:
                case Direction8.DownRight:
                    var clockwiseVec = direction.Rotate45Clockwise().Vector();
                    var anticlockwiseVec = direction.Rotate45AntiClockwise().Vector();
                    for (var i = 0; i <= Radius; i++)
                    {
                        for (var j = 0; j <= Radius; j++)
                        {
                            if (i * i + j * j <= (Radius + 0.5f) * (Radius + 0.5f))
                            {
                                area.Add(position + i * clockwiseVec + j * anticlockwiseVec);
                            }
                        }
                    }

                    break;
            }

            if (!ContainsSelf)
                area.Remove(position);
            if (CanIgnoreWalls || Radius <= 1)
                return area;
            var reachable = map.ComputeCircle(position => !map.At(position).IsBlank(EntityLayer.Middle), position,
                Radius + 0.5f);
            return area.Where(p => reachable.Contains(p));
        }

        public float EvaluateArea()
        {
            return CommonSenseParameters.CircleAreaEvaluate(CanIgnoreWalls, Radius) / 2;
        }

        public string Info()
        {
            var info = $"前{ItemDescriptionRichText.RichSpatial(Radius)}マス（扇形）";
            if (ContainsSelf) info += "(中心含む)";
            if (CanIgnoreWalls) info += "(壁無視)";
            return info;
        }
    }
}