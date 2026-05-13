#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Item;
using UnityEngine;
using Utilities;
using Utilities.Serialize;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class BaseItemMemento
    {
        [SerializeField] private string _id;
        public Id<IItem> Id => new Id<IItem>(_id);
        [field: SerializeField] public string BaseName { get; private set; }
        [field: SerializeField] public Option<string> CustomName { get; private set; }
        [field: SerializeField] public Rarity Rarity { get; private set; }
        [field: SerializeField] public Option<int> CustomBasePrice { get; private set; }
        [SerializeField] private string _iconName;
        public Sprite Icon => ObjectLoader.LoadIcon(_iconName);
        [field: SerializeField] public bool IsShiny { get; private set; }
        [field: SerializeField] public int AdditionalPrice { get; private set; }
        [field: SerializeField] public float MultiplyPrice { get; private set; }
        [field: SerializeField] public ItemState State { get; private set; }
        [field: SerializeField] public int UpgradeCount { get; private set; }
        [field: SerializeField] public int MaxUsages { get; private set; }
        [field: SerializeField] public int RemainingUsages { get; private set; }
        [field: SerializeField] public bool IsCursed { get; private set; }
        [field: SerializeField] public bool IsCurseIdentified { get; private set; }
        [field: SerializeField] public int UpgradeLimit { get; private set; }
        [field: SerializeField] public float UsageLossChance { get; private set; }
        [field: SerializeReference] public List<IConditionData> Conditions { get; private set; }
        [SerializeField] private Option<ScriptableObjectSerializable<EnemyData>> _mimic;
        public Option<EnemyData> Mimic => _mimic.Map(m => m.Value);
        [field: SerializeField] public Option<bool> IsEquipped { get; private set; }
        public BaseItemMemento(
            Id<IItem> id,
            string baseName,
            Option<string> customName,
            Rarity rarity,
            Option<int> customBasePrice,
            Sprite icon,
            bool isShiny,
            int additionalPrice,
            float multiplyPrice,
            ItemState state,
            int upgradeCount,
            int maxUsages,
            int remainingUsages,
            bool isCursed,
            bool isCurseIdentified,
            int upgradeLimit,
            float usageLossChance,
            List<IConditionData> conditions,
            Option<EnemyData> mimic,
            Option<bool> isEquipped)
        {
            _id = id.ToString();
            BaseName = baseName;
            CustomName = customName;
            Rarity = rarity;
            CustomBasePrice = customBasePrice;
            _iconName = icon.name;
            IsShiny = isShiny;
            AdditionalPrice = additionalPrice;
            MultiplyPrice = multiplyPrice;
            State = state;
            UpgradeCount = upgradeCount;
            MaxUsages = maxUsages;
            RemainingUsages = remainingUsages;
            IsCursed = isCursed;
            IsCurseIdentified = isCurseIdentified;
            UpgradeLimit = upgradeLimit;
            UsageLossChance = usageLossChance;
            Conditions = conditions;
            _mimic = mimic.Map(m => m.ToSerializable());
            IsEquipped = isEquipped;
        }

        public BaseItemMemento CopyWith(
            Id<IItem>? id = null,
            string? baseName = null,
            Option<string>? customName = null,
            Rarity? rarity = null,
            Option<int>? customBasePrice = null,
            Sprite? icon = null,
            bool? isShiny = null,
            int? additionalPrice = null,
            float? multiplyPrice = null,
            ItemState? state = null,
            int? upgradeCount = null,
            int? maxUsages = null,
            int? remainingUsages = null,
            bool? isCursed = null,
            bool? isCurseIdentified = null,
            int? upgradeLimit = null,
            float? usageLossChance = null,
            List<IConditionData>? conditions = null,
            Option<EnemyData>? mimic = null,
            Option<bool>? isEquipped = null)
        {
            return new BaseItemMemento(
                id ?? Id,
                baseName ?? BaseName,
                customName ?? CustomName,
                rarity ?? Rarity,
                customBasePrice ?? CustomBasePrice,
                icon ?? Icon,
                isShiny ?? IsShiny,
                additionalPrice ?? AdditionalPrice,
                multiplyPrice ?? MultiplyPrice,
                state ?? State,
                upgradeCount ?? UpgradeCount,
                maxUsages ?? MaxUsages,
                remainingUsages ?? RemainingUsages,
                isCursed ?? IsCursed,
                isCurseIdentified ?? IsCurseIdentified,
                upgradeLimit ?? UpgradeLimit,
                usageLossChance ?? UsageLossChance,
                conditions ?? Conditions,
                mimic ?? Mimic,
                isEquipped ?? IsEquipped
            );
        }
    }
}