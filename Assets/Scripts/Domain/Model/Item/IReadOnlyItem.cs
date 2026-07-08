#nullable enable
using Domain.Model.Dungeon;
using Domain.Model.Item.Data;
using Domain.Model.Item.Description;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Model.Item
{
    public interface IReadOnlyItem
    {
        public Id<IItem> Id { get; }
        public string BaseName { get; }
        public string RevealedName { get; }
        public Sprite Icon { get; }
        public bool IsShiny { get; }
        public ItemState State { get; }
        public bool HasActivatableSkill { get; }
        public bool CanAttemptUseOrThrow { get; }
        public Option<bool> IsEquipped { get; }
        public ReadOnlyReactiveProperty<int> RemainingUses { get; }
        public bool IsCursed { get; }
        public bool IsCurseIdentified { get; }
        public int GetPrice(ItemMarketPriceTable market);
        public ItemName GetName(IItemKnowledge knowledge, IItemPlaceholderAssigner itemPlaceholders);
        public ItemDescription Describe(IItemKnowledge knowledge, IItemPlaceholderAssigner itemPlaceholders);
        public ItemDescription DescribeIdentified();
        public ItemName NameIn(IReadOnlyMap map);
        public ItemEntityLabel LabelIn(IReadOnlyMap map);
        public ItemLook LookIn(IReadOnlyMap map);
    }
}
