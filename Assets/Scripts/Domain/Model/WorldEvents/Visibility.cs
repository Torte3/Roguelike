#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Model.WorldEvents
{
    public static class Visibility
    {
        public static bool At(IReadOnlyMap map, Vector2Int position)
        {
            return map.PlayerCharacter.IsVisible(position);
        }

        public static bool Within(IReadOnlyMap map, IEnumerable<Vector2Int> area)
        {
            return area.Any(map.PlayerCharacter.IsVisible);
        }
    }
}
