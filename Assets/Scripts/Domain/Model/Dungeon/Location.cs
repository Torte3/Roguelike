using System;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Dungeon
{
    [Serializable]
    public class Location
    {
        [SerializeField] private string _mapId;
        public Id<IMap> MapId => new(_mapId);
        [field: SerializeField] public Vector2Int Position { get; private set; }

        public Location(Id<IMap> mapId, Vector2Int position)
        {
            _mapId = mapId.ToString();
            Position = position;
        }
    }
}
