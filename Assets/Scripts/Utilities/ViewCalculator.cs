using System;
using System.Collections.Generic;
using Unity.Logging;
using UnityEngine;

namespace Utilities
{
    public static class ViewCalculator
    {
        public static HashSet<Vector2Int> ComputeFullVisibility(HashSet<Vector2Int> passables)
        {
            HashSet<Vector2Int> result = new(passables);
            foreach (var pos in passables)
            {
                for (var dx = -1; dx <= 1; dx++)
                {
                    for (var dy = -1; dy <= 1; dy++)
                    {
                        result.Add(new Vector2Int(pos.x + dx, pos.y + dy));
                    }
                }
            }

            return result;
        }

        public static HashSet<Vector2Int> FieldOfView(Vector2Int position, Vector2Int mapSize,
            Func<Vector2Int, bool> funcTileBlocked)
        {
            Log.Debug($"[View]Calculate fieldOfView from {position}");
            HashSet<Vector2Int> visited = new()
            {
                position
            };

            var minExtentX = position.x;
            var maxExtentX = mapSize.x - position.x - 1;
            var minExtentY = position.y;
            var maxExtentY = mapSize.y - position.y - 1;

            __checkQuadrant(visited, position, 1, 1, maxExtentX, maxExtentY, funcTileBlocked);
            __checkQuadrant(visited, position, 1, -1, maxExtentX, minExtentY, funcTileBlocked);
            __checkQuadrant(visited, position, -1, -1, minExtentX, minExtentY, funcTileBlocked);
            __checkQuadrant(visited, position, -1, 1, minExtentX, maxExtentY, funcTileBlocked);

            return visited;
        }

        public static HashSet<Vector2Int> FieldOfView(Vector2Int position, int viewDistance,
            Func<Vector2Int, bool> funcTileBlocked)
        {
            Log.Debug($"[View]Calculate fieldOfView from {position}");
            HashSet<Vector2Int> visited = new()
            {
                position
            };

            var minExtentX = viewDistance;
            var maxExtentX = viewDistance;
            var minExtentY = viewDistance;
            var maxExtentY = viewDistance;

            __checkQuadrant(visited, position, 1, 1, maxExtentX, maxExtentY, funcTileBlocked);
            __checkQuadrant(visited, position, 1, -1, maxExtentX, minExtentY, funcTileBlocked);
            __checkQuadrant(visited, position, -1, -1, minExtentX, minExtentY, funcTileBlocked);
            __checkQuadrant(visited, position, -1, 1, minExtentX, maxExtentY, funcTileBlocked);

            return visited;
        }

        [Serializable]
        public class __Line
        {
            public int xi, yi, xf, yf;

            public __Line(int xi, int yi, int xf, int yf)
            {
                this.xi = xi;
                this.yi = yi;
                this.xf = xf;
                this.yf = yf;
            }

            public int dx => xf - xi;
            public int dy => yf - yi;

            public bool pBelow(int x, int y)
            {
                return relativeSlope(x, y) > 0;
            }

            public bool pBelowOrCollinear(int x, int y)
            {
                return relativeSlope(x, y) >= 0;
            }

            public bool pAbove(int x, int y)
            {
                return relativeSlope(x, y) < 0;
            }

            public bool pAboveOrCollinear(int x, int y)
            {
                return relativeSlope(x, y) <= 0;
            }

            public bool pCollinear(int x, int y)
            {
                return relativeSlope(x, y) == 0;
            }

            public bool lineCollinear(__Line line)
            {
                return pCollinear(line.xi, line.yi) && pCollinear(line.xf, line.yf);
            }

            public int relativeSlope(int x, int y)
            {
                return dy * (xf - x) - dx * (yf - y);
            }

            public __Line DeepCopy()
            {
                return new __Line(xi, yi, xf, yf);
            }
        }

        [Serializable]
        public class __ViewBump
        {
            public int x, y;
            public __ViewBump parent;

            public __ViewBump(int x, int y, __ViewBump parent)
            {
                this.x = x;
                this.y = y;
                this.parent = parent;
            }

            public __ViewBump DeepCopy()
            {
                return new __ViewBump(x, y, parent?.DeepCopy());
            }
        }

        [Serializable]
        public class __View
        {
            public __Line shallowLine, steepLine;
            public __ViewBump shallowBump, steepBump;

            public __View(__Line shallowLine, __Line steepLine)
            {
                this.shallowLine = shallowLine;
                this.steepLine = steepLine;
                shallowBump = null;
                steepBump = null;
            }

            public __View DeepCopy()
            {
                return new __View(shallowLine.DeepCopy(), steepLine.DeepCopy())
                {
                    shallowBump = shallowBump?.DeepCopy(),
                    steepBump = steepBump?.DeepCopy()
                };
            }
        }

        public static void __checkQuadrant(HashSet<Vector2Int> visited, Vector2Int start, int dx, int dy, int extentX,
            int extentY, Func<Vector2Int, bool> funcTileBlocked)
        {
            List<__View> activeViews = new();

            var shallowLine = new __Line(0, 1, extentX, 0);
            var steepLine = new __Line(1, 0, 0, extentY);

            activeViews.Add(new __View(shallowLine, steepLine));
            var viewIndex = 0;

            var maxI = extentX + extentY;
            for (var i = 1; i <= maxI; i++)
            {
                if (activeViews.Count == 0)
                    break;
                var startJ = Mathf.Max(0, i - extentX);
                var maxJ = Mathf.Min(i, extentY);

                for (var j = startJ; j <= maxJ; j++)
                {
                    if (viewIndex >= activeViews.Count)
                        break;
                    var x = i - j;
                    var y = j;
                    __visitCoord(visited, start, x, y, dx, dy, viewIndex, activeViews, funcTileBlocked);
                }
            }
        }

        public static void __visitCoord(HashSet<Vector2Int> visited, Vector2Int start, int x, int y, int dx, int dy,
            int viewIndex, List<__View> activeViews, Func<Vector2Int, bool> funcTileBlocked)
        {
            var topLeft = new Vector2Int(x, y + 1);
            var bottomRight = new Vector2Int(x + 1, y);

            while (viewIndex < activeViews.Count &&
                   activeViews[viewIndex].steepLine.pBelowOrCollinear(bottomRight.x, bottomRight.y))
            {
                viewIndex += 1;
            }

            if (viewIndex == activeViews.Count ||
                activeViews[viewIndex].shallowLine.pAboveOrCollinear(topLeft.x, topLeft.y))
                return;

            var real = new Vector2Int(x * dx, y * dy);

            if (!visited.Contains(start + real))
            {
                visited.Add(start + real);
            }

            var isBlocked = funcTileBlocked(start + real);

            if (!isBlocked)
                return;

            if (activeViews[viewIndex].shallowLine.pAbove(bottomRight.x, bottomRight.y) &&
                activeViews[viewIndex].steepLine.pBelow(topLeft.x, topLeft.y))
                activeViews.RemoveAt(viewIndex);
            else if (activeViews[viewIndex].shallowLine.pAbove(bottomRight.x, bottomRight.y))
            {
                __addShallowBump(topLeft.x, topLeft.y, activeViews, viewIndex);
                __checkView(activeViews, viewIndex);
            }
            else if (activeViews[viewIndex].steepLine.pBelow(topLeft.x, topLeft.y))
            {
                __addSteepBump(bottomRight.x, bottomRight.y, activeViews, viewIndex);
                __checkView(activeViews, viewIndex);
            }
            else
            {
                var shallowViewIndex = viewIndex;
                viewIndex += 1;
                var steepViewIndex = viewIndex;

                activeViews.Insert(shallowViewIndex, activeViews[shallowViewIndex].DeepCopy());

                __addSteepBump(bottomRight.x, bottomRight.y, activeViews, shallowViewIndex);
                if (!__checkView(activeViews, shallowViewIndex))
                {
                    viewIndex -= 1;
                    steepViewIndex -= 1;
                }

                __addShallowBump(topLeft.x, topLeft.y, activeViews, steepViewIndex);
                __checkView(activeViews, steepViewIndex);
            }
        }

        public static void __addShallowBump(int x, int y, List<__View> activeViews, int viewIndex)
        {
            activeViews[viewIndex].shallowLine.xf = x;
            activeViews[viewIndex].shallowLine.yf = y;

            activeViews[viewIndex].shallowBump = new __ViewBump(x, y, activeViews[viewIndex].shallowBump);

            var curBump = activeViews[viewIndex].steepBump;
            while (curBump != null)
            {
                if (activeViews[viewIndex].shallowLine.pAbove(curBump.x, curBump.y))
                {
                    activeViews[viewIndex].shallowLine.xi = curBump.x;
                    activeViews[viewIndex].shallowLine.yi = curBump.y;
                }

                curBump = curBump.parent;
            }
        }

        public static void __addSteepBump(int x, int y, List<__View> activeViews, int viewIndex)
        {
            activeViews[viewIndex].steepLine.xf = x;
            activeViews[viewIndex].steepLine.yf = y;

            activeViews[viewIndex].steepBump = new __ViewBump(x, y, activeViews[viewIndex].steepBump);

            var curBump = activeViews[viewIndex].shallowBump;
            while (curBump != null)
            {
                if (activeViews[viewIndex].steepLine.pBelow(curBump.x, curBump.y))
                {
                    activeViews[viewIndex].steepLine.xi = curBump.x;
                    activeViews[viewIndex].steepLine.yi = curBump.y;
                }

                curBump = curBump.parent;
            }
        }

        public static bool __checkView(List<__View> activeViews, int viewIndex)
        {
            var shallowLine = activeViews[viewIndex].shallowLine;
            var steepLine = activeViews[viewIndex].steepLine;

            if (shallowLine.lineCollinear(steepLine) && (shallowLine.pCollinear(0, 1) || shallowLine.pCollinear(1, 0)))
            {
                activeViews.RemoveAt(viewIndex);
                return false;
            }

            return true;
        }
    }
}