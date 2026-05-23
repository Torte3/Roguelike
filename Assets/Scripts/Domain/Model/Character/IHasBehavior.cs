using System.Collections.Generic;
using Domain.Model.Effect;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Character
{
    public interface IHasBehavior : IActor, ITargetOfEffect
    {
        public bool CanPickUp { get; }
        public bool CanUseItem { get; }
        public bool CanReceivePlayerGift { get; }
        public IReadOnlyList<ICharacterSkillWithRule> Skills { get; }
        public bool CanSwap(Vector2Int position, Direction8 direction, IMap map);
        public bool TryPickUpItem(IMap map, bool canPickUpShopItem);
    }
}