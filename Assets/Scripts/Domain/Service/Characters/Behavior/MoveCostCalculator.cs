using Domain.Model.Character;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    internal class MoveCostCalculator
    {
        private IHasBehavior _character;
        private IMap _map;
        private bool _canSwap;

        public MoveCostCalculator(IHasBehavior character, IMap map, bool canSwap)
        {
            _character = character;
            _map = map;
            _canSwap = canSwap;
        }

        public float Calculate(Vector2Int pos, Direction8 direction)
        {
            var next = pos + direction.Vector();
            if (_map.GetCharacterAt(next) != null && !_character.IsVisible(next))
                return _character.CanMoveIgnoreEntity(pos, direction, _character.KnownTerrain) ? 1 : float.PositiveInfinity;
            if (_character.CanMove(pos, direction, _character.KnownTerrain))
                return 1;
            if (_canSwap && _character.CanSwap(pos, direction, _map))
                return 2;
            return float.PositiveInfinity;
        }
    }
}