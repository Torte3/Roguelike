using System;
using System.Collections.Generic;
using System.Linq;
using StringSerializableEnum;
using Utilities;
using ApplicabilityTag = Domain.Model.Item.FeatureApplicabilityTag;

namespace Domain.Model.Item
{
    [Flags]
    public enum FeatureApplicabilityTag
    {
        None = 0,
        DirectWeapons = 1 << 0,
        RangedWeapons = 1 << 1,
        Weapons = DirectWeapons | RangedWeapons,
        Potions = 1 << 2,
        Scrolls = 1 << 3,
        Wandss = 1 << 4,
        Books = 1 << 5,
        Others = 1 << 6,
        All = ~0
    }
    [StringSerializable]
    public enum ItemFeature
    {
        //DirectWeapon AttackArea
        TwoRangeAttack,
        FanAttack,
        SpinAttack,
        
        Lunge,
        ChargeAttack,

        //RangedWeapon AttackPosition
        ArcingShot,
        Piercing,

        //RangedWeapon AttackArea
        Explosive,

        //Attack Additional
        DoubleAttack,
        TripleAttack,
        Knockback,
        Critical,
        Dig,
        BreakTrap,
        Absorbing,
        GuaranteedHit,
        EnhanceThrow,
        Paralysis,
        Blind,
        Confusion,
        Sleep,
        Poison,
        Slowness,
        Restraint,
        EnhanceAbnormalCondition,

        //Attack Element
        Fire,
        Ice,
        Thunder,
        Light,
        Dark,

        //Other
        EnhanceDurability,
        Artistic,
    }
    public static class DirectWeaponFeatureExtensions
    {
        /// <summary> 順序なし。各グループ内のどれか1つしか付けられない。 </summary>
        private static readonly IReadOnlyList<IReadOnlyList<ItemFeature>> ExclusionGroups = new[]
        {
            new[] { ItemFeature.TwoRangeAttack, ItemFeature.FanAttack, ItemFeature.SpinAttack, ItemFeature.Explosive },
            new[] { ItemFeature.ArcingShot, ItemFeature.Piercing },
            new[] { ItemFeature.Fire, ItemFeature.Ice, ItemFeature.Thunder, ItemFeature.Light, ItemFeature.Dark },
        };

        /// <summary> 順序あり（リストの後ろほど上位）。上位を付けると下位は上書きされる。 </summary>
        private static readonly IReadOnlyList<IReadOnlyList<ItemFeature>> SupersededFeatureGroups = new[]
        {
            new[] { ItemFeature.DoubleAttack, ItemFeature.TripleAttack },
        };

        private static IReadOnlyList<ItemFeature>? GetExclusionGroup(this ItemFeature feature)
        {
            return ExclusionGroups.FirstOrDefault(g => g.Contains(feature));
        }

        private static (IReadOnlyList<ItemFeature> group, int rank)? GetSupersededGroupAndRank(this ItemFeature feature)
        {
            foreach (var group in SupersededFeatureGroups)
            {
                var idx = group.IndexOf(feature);
                if (idx >= 0)
                    return (group, idx);
            }
            return null;
        }

        public static string GetName(this ItemFeature feature)
        {
            return feature switch
            {
                ItemFeature.TwoRangeAttack => "2マス攻撃",
                ItemFeature.FanAttack => "扇型攻撃",
                ItemFeature.SpinAttack => "回転攻撃",
                
                ItemFeature.Lunge => "突進",
                ItemFeature.ChargeAttack => "溜め攻撃",

                ItemFeature.ArcingShot => "曲射",
                ItemFeature.Piercing => "貫通",

                ItemFeature.Explosive => "爆発",

                ItemFeature.DoubleAttack => "2回攻撃",
                ItemFeature.TripleAttack => "3回攻撃",
                ItemFeature.Knockback => "吹き飛ばし",
                ItemFeature.Critical => "クリティカル",
                ItemFeature.Dig => "掘る",
                ItemFeature.BreakTrap => "トラップを破壊",
                ItemFeature.Absorbing => "吸収",
                ItemFeature.GuaranteedHit => "必中",
                ItemFeature.EnhanceThrow => "投擲強化",
                ItemFeature.Paralysis => "麻痺",
                ItemFeature.Blind => "盲目",
                ItemFeature.Confusion => "混乱",
                ItemFeature.Sleep => "眠り",
                ItemFeature.Poison => "毒",
                ItemFeature.Slowness => "鈍足",
                ItemFeature.Restraint => "拘束",
                ItemFeature.EnhanceAbnormalCondition => "状態異常付与率強化",

                ItemFeature.Fire => "火",
                ItemFeature.Ice => "氷",
                ItemFeature.Thunder => "雷",
                ItemFeature.Light => "光",
                ItemFeature.Dark => "闇",

                ItemFeature.EnhanceDurability => "耐久強化",
                ItemFeature.Artistic => "美術品",
                _ => throw new Exception("Invalid DirectWeaponFeature")
            };
        }
        public static ApplicabilityTag GetApplicability(this ItemFeature feature)
        {
            return feature switch
            {
                ItemFeature.TwoRangeAttack => ApplicabilityTag.DirectWeapons,
                ItemFeature.FanAttack => ApplicabilityTag.DirectWeapons,
                ItemFeature.SpinAttack => ApplicabilityTag.DirectWeapons,

                ItemFeature.Lunge => ApplicabilityTag.DirectWeapons,
                ItemFeature.ChargeAttack => ApplicabilityTag.Weapons,

                ItemFeature.ArcingShot => ApplicabilityTag.RangedWeapons,
                ItemFeature.Piercing => ApplicabilityTag.RangedWeapons,

                ItemFeature.Explosive => ApplicabilityTag.RangedWeapons,

                ItemFeature.DoubleAttack => ApplicabilityTag.Weapons,
                ItemFeature.TripleAttack => ApplicabilityTag.Weapons,
                ItemFeature.Knockback => ApplicabilityTag.Weapons,
                ItemFeature.Critical => ApplicabilityTag.Weapons,
                ItemFeature.Dig => ApplicabilityTag.Weapons,
                ItemFeature.BreakTrap => ApplicabilityTag.Weapons,
                ItemFeature.Absorbing => ApplicabilityTag.Weapons,
                ItemFeature.GuaranteedHit => ApplicabilityTag.Weapons,
                ItemFeature.EnhanceThrow => ApplicabilityTag.Weapons,
                ItemFeature.Paralysis => ApplicabilityTag.Weapons,
                ItemFeature.Blind => ApplicabilityTag.Weapons,
                ItemFeature.Confusion => ApplicabilityTag.Weapons,
                ItemFeature.Sleep => ApplicabilityTag.Weapons,
                ItemFeature.Poison => ApplicabilityTag.Weapons,
                ItemFeature.Slowness => ApplicabilityTag.Weapons,
                ItemFeature.Restraint => ApplicabilityTag.Weapons,
                ItemFeature.EnhanceAbnormalCondition => ApplicabilityTag.Weapons,

                ItemFeature.Fire => ApplicabilityTag.Weapons,
                ItemFeature.Ice => ApplicabilityTag.Weapons,
                ItemFeature.Thunder => ApplicabilityTag.Weapons,
                ItemFeature.Light => ApplicabilityTag.Weapons,
                ItemFeature.Dark => ApplicabilityTag.Weapons,

                ItemFeature.EnhanceDurability => ApplicabilityTag.Weapons,
                ItemFeature.Artistic => ApplicabilityTag.Weapons,
                _ => throw new Exception("Invalid DirectWeaponFeature")
            };
        }
        public static bool CanAdd(this IEnumerable<ItemFeature> features, ItemFeature feature, ApplicabilityTag targetApplicability)
        {
            var featureApplicability = feature.GetApplicability();
            if (!featureApplicability.HasFlag(targetApplicability))
            {
                return false;
            }

            var exclusionGroup = feature.GetExclusionGroup();
            if (exclusionGroup != null)
                return !features.Any(f => exclusionGroup.Contains(f));

            var superseded = feature.GetSupersededGroupAndRank();
            if (superseded != null)
            {
                var (group, myRank) = superseded.Value;
                var hasHigher = features.Any(f => group.IndexOf(f) is var r && r >= 0 && r > myRank);
                return !hasHigher;
            }

            const int CANNOT_OVERLAP = 1;
            var sameCount = features.Count(f => f == feature);
            var maxOverlap = feature switch
            {
                ItemFeature.Lunge => CANNOT_OVERLAP,
                ItemFeature.ChargeAttack => CANNOT_OVERLAP,
                ItemFeature.Knockback => CANNOT_OVERLAP,
                ItemFeature.Critical => 4,
                ItemFeature.Dig => CANNOT_OVERLAP,
                ItemFeature.BreakTrap => CANNOT_OVERLAP,
                ItemFeature.Absorbing => 4,
                ItemFeature.GuaranteedHit => CANNOT_OVERLAP,
                ItemFeature.EnhanceThrow => CANNOT_OVERLAP,
                ItemFeature.Paralysis => CANNOT_OVERLAP,
                ItemFeature.Blind => CANNOT_OVERLAP,
                ItemFeature.Confusion => CANNOT_OVERLAP,
                ItemFeature.Sleep => CANNOT_OVERLAP,
                ItemFeature.Poison => CANNOT_OVERLAP,
                ItemFeature.Slowness => CANNOT_OVERLAP,
                ItemFeature.Restraint => CANNOT_OVERLAP,
                ItemFeature.EnhanceAbnormalCondition => 4,

                ItemFeature.EnhanceDurability => 5,
                ItemFeature.Artistic => CANNOT_OVERLAP,
                _ => throw new Exception($"ItemFeature {feature} should belong to ExclusionGroup, SupersededFeatureGroup, or have valid overlap count")
            };
            return sameCount < maxOverlap;
        }
        private static IOrderedEnumerable<ItemFeature> Merge(this IEnumerable<ItemFeature> features, ItemFeature otherFeature, ApplicabilityTag targetApplicability)
        {
            var allFeatures = features.ToList();

            if (allFeatures.CanAdd(otherFeature, targetApplicability))
            {
                var superseded = otherFeature.GetSupersededGroupAndRank();
                if (superseded != null)
                {
                    var (group, myRank) = superseded.Value;
                    allFeatures.RemoveAll(f => group.IndexOf(f) is var r && r >= 0 && r < myRank);
                }
                allFeatures.Add(otherFeature);
            }

            return allFeatures.OrderBy(f => f);
        }
        public static IOrderedEnumerable<ItemFeature> Merge(this IEnumerable<ItemFeature> features, IEnumerable<ItemFeature> otherFeatures, int maxFeatureCount, ApplicabilityTag targetApplicability)
        {
            var result = features.OrderBy(f => f);
            foreach (var feature in otherFeatures)
            {
                if (result.Count() >= maxFeatureCount)
                    break;
                if (result.CanAdd(feature, targetApplicability))
                {
                    result = result.Merge(feature, targetApplicability);
                }
            }

            return result;
        }
    }
}
