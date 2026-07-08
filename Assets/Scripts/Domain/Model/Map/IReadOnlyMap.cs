#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Item.Data;
using Domain.Model.WorldEvents;
using UnityEngine;

namespace Domain.Model.Map
{
    public interface IReadOnlyMap
    {
        public IReadOnlyPlayerCharacter PlayerCharacter { get; }
        public IItemPlaceholderAssigner ItemPlaceholders { get; }
        public ItemMarketPriceTable MarketPriceTable { get; }
        public IReadOnlyShop? Shop { get; }
        public IReadOnlyItem? ItemAt(Vector2Int position);
        public IEnumerable<IReadOnlyItem> ItemsIn(IEnumerable<Vector2Int> positions);
        public Underfoot PlayerUnderfoot();
    }
}
