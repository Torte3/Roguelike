using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IPassableChecker
    {
        public IMapPosition At(Vector2Int position);
        public IEnumerable<IMapPosition> GetAllBlankPositions();
        public IEnumerable<IMapPosition> GetAllBlankPositionsOn(params EntityLayer[] layers);
        public IEnumerable<IMapPosition> GetAllBlankAndStandablePositions();
        public IEnumerable<IMapPosition> GetAllBlankAndStandablePositionsOn(params EntityLayer[] layers);
        public IEnumerable<IMapPosition> GetAllWalkablePositions(IAffiliation affiliation);
    }
}