using Domain.Model.Map;
using UnityEngine;

namespace Domain.Model.Effect
{
    public interface IPassableChecker
    {
        public IMapPosition At(Vector2Int position);
    }
}