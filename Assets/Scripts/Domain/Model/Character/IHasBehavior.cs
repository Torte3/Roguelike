using System.Collections.Generic;
using Domain.Model.Effect;
using Domain.Model.Map;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Model.Character
{
    public interface IHasBehavior : IActor, ITargetOfEffect
    {
        public bool CanPickUp { get; }
        public bool CanUseItem { get; }
        public bool CanReceivePlayerGift { get; }
        public IReadOnlyList<ICharacterSkillWithRule> Skills { get; }
        public bool CanSwap(Vector2Int position, Direction8 direction, IMap map);
        public bool CanMoveIgnoreEntity(Vector2Int position, Direction8 direction, IPassableChecker map);
        public IPassableChecker KnownTerrain { get; }
        public Route RouteTo(Vector2Int destination, IMap map);
        public bool AcceptsSwapFrom(Vector2Int requesterPosition, IMap map);
        public bool IsVisible(Vector2Int position);
    }
}