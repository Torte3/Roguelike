#nullable enable
using Domain.Model.Character;
using Domain.Model.Entity;
using UnityEngine;

namespace Domain.Model.Map
{
    public interface IMapPosition
    {
        public Vector2Int Position { get; init; }
        public bool IsBlankIgnoreWall(params EntityLayer[] layers);
        public bool IsBlank(params EntityLayer[] layers);
        public bool IsBlankAndStandable(params EntityLayer[] layers);

        public bool CanPlace(bool isFlying, bool canThroughWalls, bool ignoreEntity,
            params EntityLayer[] layers);

        public bool IsWalkable(IAffiliation actor);
        public bool IsWalkableOnMap();
        public bool IsPassableOnMap();
        public bool IsLightPassable();
    }
}