#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Effect;
using Domain.Service.Logs;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public abstract class BaseItem : IItem, IDisposable
    {
        public Id<IItem> Id { get; private set; }
        public string BaseName { get; private set; }
        public Option<string> CustomName { get; private set; }
        public Rarity Rarity { get; private set; }
        public Sprite Icon { get; private set; }
        public bool IsShiny { get; private set; }
        private Option<int> _customBasePrice { get; init; }
        public int _additionalPrice { get; private set; }
        public float _multiplyPrice { get; private set; }
        public ItemState State { get; private set; }
        public int UpgradeCount { get; private protected set; }
        public int MaxUsages { get; private set; }
        private protected ReactiveProperty<int> _remainingUsages;
        public float UsageLossChance { get; private set; }
        private protected ReactiveProperty<bool> _isCursed;
        public ReadOnlyReactiveProperty<bool> Cursed => _isCursed;
        public bool IsCursed => _isCursed.CurrentValue;
        private protected ReactiveProperty<bool> _isCurseIdentified;
        public ReadOnlyReactiveProperty<bool> CurseIdentified => _isCurseIdentified;
        public bool IsCurseIdentified => _isCurseIdentified.CurrentValue;
        public int UpgradeLimit { get; private set; }
        private protected List<IConditionData> _conditions;
        private protected Subject<Unit> _onItemUpdated = new();
        private protected Subject<Unit> _onMimicRevealed = new();
        private protected CompositeDisposable _disposables = new();
        protected bool UsedWhileCursed { get; private protected set; }
        public abstract ItemCategory Category { get; }
        public abstract string RevealedName { get; }
        protected abstract bool HasSameEffect { get; }
        protected abstract bool HasSameSkill { get; }
        public abstract bool UseOnDeath { get; }
        public abstract bool RequiresLiteracy { get; }
        public abstract bool IdentifyIfGot { get; }
        public abstract bool IdentifyIfUsed { get; }
        public abstract bool AutoDestroyWhenDisabled { get; }
        public abstract Option<ISkillWithCost> SkillOnUse { get; }
        public abstract Option<ISkillWithCost> SkillOnThrow { get; }
        private Option<EnemyData> _mimic { get; init; }

        protected bool HasUsableSkillOnUse() => SkillOnUse.HasValue && SkillOnUse.Value.IsUsable();
        protected bool HasUsableSkillOnThrow() => SkillOnThrow.HasValue && SkillOnThrow.Value.IsUsable();

        public string DebugName => _fullName;
        private string _fullName => CustomName.UnwrapOr(RevealedName) + _upgradeText();
        private string _upgradeText()
        {
            if (UpgradeCount == 0)
                return "";
            else if (UpgradeCount > 0)
                return $" +{UpgradeCount}";
            else
                return $" {UpgradeCount}";
        }
        public int GetPrice(ItemMarketPriceTable market) => Mathf.RoundToInt(EvaluatePrice(market));
        public bool HasActivatableSkillWhenUsed => SkillOnUse.HasValue;
        public bool HasActivatableSkillWhenThrown => SkillOnThrow.HasValue;
        public abstract bool CanActivateWhenUsed { get; }
        public abstract bool CanActivateWhenThrown { get; }
        public bool HasActivatableSkill => HasActivatableSkillWhenUsed || HasActivatableSkillWhenThrown;
        public bool CanActivate => CanActivateWhenUsed || CanActivateWhenThrown;

        public abstract bool CanAttemptUse { get; }

        public abstract bool CanAttemptThrow { get; }

        public bool CanAttemptUseOrThrow => CanAttemptUse || CanAttemptThrow;

        public abstract bool IsDiscardBlocked { get; }
        public abstract Option<bool> IsEquipped { get; }
        public abstract ReadOnlyReactiveProperty<bool> IsPassiveActive { get; }
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUsages;
        public IReadOnlyList<IConditionData> PassiveConditions => _conditions;
        public Observable<Unit> OnItemUpdated => _onItemUpdated;
        public Observable<Unit> OnMimicRevealed => _onMimicRevealed;

        public string UnknownName(ItemPlaceholders itemPlaceholders)
        {
            return $"?{CustomName.UnwrapOr(itemPlaceholders.GetPlaceholder(BaseName, Category))}?";
        }
        public string GetName(IPlayer player, ItemPlaceholders itemPlaceholders)
        {
            if (player.Character.IsKnownItem(this))
                return _fullName;
            return UnknownName(itemPlaceholders);
        }

        protected BaseItem(BaseItemMemento baseItem)
        {
            Id = baseItem.Id;
            BaseName = baseItem.BaseName;
            CustomName = baseItem.CustomName;
            Rarity = baseItem.Rarity;
            Icon = baseItem.Icon;
            IsShiny = baseItem.IsShiny;
            _customBasePrice = baseItem.CustomBasePrice;
            _additionalPrice = baseItem.AdditionalPrice;
            _multiplyPrice = baseItem.MultiplyPrice;
            State = baseItem.State;
            UpgradeCount = baseItem.UpgradeCount;
            MaxUsages = baseItem.MaxUsages;
            _remainingUsages = new ReactiveProperty<int>(baseItem.RemainingUsages);
            UsageLossChance = baseItem.UsageLossChance;
            _isCursed = new ReactiveProperty<bool>(baseItem.IsCursed);
            _isCurseIdentified = new ReactiveProperty<bool>(baseItem.IsCurseIdentified);
            UpgradeLimit = baseItem.UpgradeLimit;
            _conditions = baseItem.Conditions;
            _mimic = baseItem.Mimic;
            UsedWhileCursed = baseItem.UsedWhileCursed;
        }

        public BaseItemMemento SerializeBase()
        {
            return new BaseItemMemento(
                id: Id,
                baseName: BaseName,
                customName: CustomName,
                rarity: Rarity,
                customBasePrice: _customBasePrice,
                icon: Icon,
                isShiny: IsShiny,
                additionalPrice: _additionalPrice,
                multiplyPrice: _multiplyPrice,
                state: State,
                upgradeCount: UpgradeCount,
                maxUsages: MaxUsages,
                remainingUsages: _remainingUsages.CurrentValue,
                isCursed: IsCursed,
                isCurseIdentified: IsCurseIdentified,
                upgradeLimit: UpgradeLimit,
                usageLossChance: UsageLossChance,
                conditions: _conditions,
                mimic: _mimic,
                usedWhileCursed: UsedWhileCursed);
        }

        public static BaseItemMemento BuildBase(
            string baseName,
            Sprite icon,
            bool isShiny,
            Rarity rarity,
            int? customBasePrice,
            int additionalPrice,
            float multiplyPrice,
            ItemState state,
            int upgradeCount,
            int maxUsages,
            float usageLossChance,
            bool isCursed,
            int upgradeLimit,
            List<IConditionData> conditions,
            Option<EnemyData> mimic)
        {
            return new BaseItemMemento(
                id: Id<IItem>.Generate(),
                baseName: baseName,
                customName: Option<string>.None,
                rarity: rarity,
                customBasePrice: customBasePrice.ToOption(),
                icon: icon,
                isShiny: isShiny,
                additionalPrice: additionalPrice,
                multiplyPrice: multiplyPrice,
                state: state,
                upgradeCount: upgradeCount,
                maxUsages: maxUsages,
                remainingUsages: maxUsages,
                usageLossChance: usageLossChance,
                isCursed: isCursed,
                isCurseIdentified: false,
                upgradeLimit: upgradeLimit,
                conditions: conditions,
                mimic: mimic);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void SetState(ItemState state)
        {
            State = state;
            _onItemUpdated.OnNext(Unit.Default);
        }

        public bool ShouldRevealMimic(IActorOfEffect actor, Vector2Int position, IMap map)
        {
            if (_mimic.IsSome(out var mimic))
            {
                GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は{mimic.Name}の擬態だった！");
                map.SpawnEnemyIgnoreMimic(mimic, position, doActImmediately: true, isSlept: false, isShiny: false);
                _onMimicRevealed.OnNext(Unit.Default);
                return true;
            }
            return false;
        }

        public abstract UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map);

        public abstract void LogWhyCannotActivateWhenUsed(IActor actor, IMap map);

        public abstract UniTask<ISkillResult> UseWhenThrown(IActorOfEffect actor, Vector2Int position,
            Direction8 direction, IMap map);

        public float EvaluateWhenUsed(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            if (UseOnDeath)
            {
                return 0;
            }

            return SkillOnUse.MapOr(
                0,
                skill => skill.Evaluate(actor, position, direction, map, this)
            );
        }

        public float EvaluateWhenThrown(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map)
        {
            return SkillOnThrow.MapOr(
                0,
                skill => skill.Evaluate(actor, position, direction, map, this)
            );
        }

        public float EvaluateBasePrice()
        {
            var priceOnUse = SkillOnUse.MapOr(0, skill => skill.EvaluatePrice()) * (UseOnDeath ? 5 : 1);
            var priceOnThrow = SkillOnThrow.MapOr(0, skill => skill.EvaluatePrice()) *
                               CommonSenseParameters.ProjectileImpactHitProbability;
            var price = Mathf.Max(priceOnUse, priceOnThrow) * MaxUsages;
            price += _additionalPrice;
            price += _conditions.Sum(condition => condition.EvaluatePrice()) * 100;
            if (IsCursed)
            {
                price *= 0.8f;
            }

            price *= _multiplyPrice;

            return price;
        }

        public float EvaluateEvaluatedPrice()
        {
            var priceOnUse = SkillOnUse.MapOr(0, skill => skill.EvaluatePrice()) * (UseOnDeath ? 5 : 1);
            var priceOnThrow = SkillOnThrow.MapOr(0, skill => skill.EvaluatePrice()) *
                               CommonSenseParameters.ProjectileImpactHitProbability;
            var basePrice = Mathf.Max(priceOnUse, priceOnThrow);
            
            var usageMultiplier = (_remainingUsages.CurrentValue + MaxUsages) / 2 / Mathf.Max(UsageLossChance, 0.1f);
            
            var price = basePrice * usageMultiplier;
            price += _additionalPrice;
            price += _conditions.Sum(condition => condition.EvaluatePrice()) * 100;
            if (IsCursed)
            {
                price *= 0.8f;
            }

            price *= _multiplyPrice;

            return price;
        }

        public float EvaluatePrice(ItemMarketPriceTable market)
        {
            var basePrice = _customBasePrice
                .Map(customBasePrice => (float)customBasePrice)
                .UnwrapOr(() => market.GetBasePrice(Category, Rarity));

            var usagesMultiplier = MaxUsages <= 0
                ? 1f
                : (_remainingUsages.CurrentValue + MaxUsages) / Mathf.Max(1, MaxUsages) / 2f;

            var price = basePrice * usagesMultiplier;
            price += _additionalPrice;
            if (IsCursed)
            {
                price *= 0.8f;
            }

            price *= _multiplyPrice;

            return price;
        }

        public void UpdateTurn()
        {
            if (SkillOnUse.HasValue && !SkillOnUse.Value.IsUsable())
            {
                SkillOnUse.Value.CoolDown();
                if (SkillOnUse.Value.IsUsable())
                {
                    _onItemUpdated.OnNext(Unit.Default);
                }
            }
            if (SkillOnThrow.HasValue && !SkillOnThrow.Value.IsUsable())
            {
                SkillOnThrow.Value.CoolDown();
                if (SkillOnThrow.Value.IsUsable())
                {
                    _onItemUpdated.OnNext(Unit.Default);
                }
            }
        }

        public abstract void Repair(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders);

        public void SetCursed(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool isCursed)
        {
            if (isCursed)
            {
                GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}は呪われた");
            }
            else
            {
                GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}の呪いは解かれた");
            }

            if (_isCursed.CurrentValue == isCursed)
            {
                SetCurseIdentified(true);
                return;
            }

            _isCursed.Value = isCursed;
            if (!isCursed)
            {
                UsedWhileCursed = false;
            }

            SetCurseIdentified(true);
            _onItemUpdated.OnNext(Unit.Default);
        }

        public void SetCurseIdentified(bool isCurseIdentified, IPlayer? logPlayer = null,
            IEntity? logVisibleEntity = null, ItemPlaceholders? logPlaceholders = null)
        {
            var wasUnidentified = !IsCurseIdentified;
            _isCurseIdentified.Value = isCurseIdentified;
            if (isCurseIdentified && wasUnidentified && IsCursed
                && logPlayer != null && logVisibleEntity != null && logPlaceholders != null)
            {
                GameLog.Add(logVisibleEntity.IsVisible,
                    $"{GetName(logPlayer, logPlaceholders)}は呪われていた");
            }

            _onItemUpdated.OnNext(Unit.Default);
        }

        public void Rename(string name)
        {
            CustomName = Option.Some(name);
            _onItemUpdated.OnNext(Unit.Default);
        }

        public void RevertToDefaultName()
        {
            CustomName = Option.None<string>();
            _onItemUpdated.OnNext(Unit.Default);
        }

        #region Upgrade

        public abstract bool CanUpgrade();
        public abstract bool CanDowngrade();
        public abstract void Upgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool log = true);
        public abstract void Downgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool log = true);

        #endregion
        #region Info
        public bool IsInfoIdentified(IPlayer player)
        {
            return player.Character.IsKnownItem(this);
        }

        public string CursedInfo()
        {
            if (IsCurseIdentified)
            {
                if (IsCursed)
                    return ItemDescriptionRichText.HarmfulLine("それは呪われている") + "\n";
                return "それは呪われていない\n";
            }

            return "それは呪われているかわからない\n";
        }

        public string Info(IPlayer player, ItemPlaceholders itemPlaceholders)
        {
            if (IsInfoIdentified(player))
            {
                return FullInfo();
            }
            else
            {
                return UnknownInfo(itemPlaceholders);
            }
        }

        public string UnknownInfo(ItemPlaceholders itemPlaceholders)
        {
            var info = $"{State.GetDescription()}{UnknownName(itemPlaceholders)}\n";
            info += CursedInfo();
            if (HasActivatableSkillWhenUsed)
                info += "それは使用可能である\n";
            if (HasActivatableSkillWhenThrown)
                info += "それは投擲可能である\n";
            return info;
        }

        public string DebugInfo()
        {
            return FullInfo();
        }

        protected abstract string FullInfoImpl();

        protected virtual string? BuildTemplatedActivatableSkillInfo() => null;

        public string PreviewTemplatedSkillSection() => BuildTemplatedActivatableSkillInfo() ?? "";

        public string FullInfo() => BuildFullInfo(useActivatableSkillTemplate: true);

        public string FullInfoGenericSkillDescription() => BuildFullInfo(useActivatableSkillTemplate: false);

        private string BuildFullInfo(bool useActivatableSkillTemplate)
        {
            var info = $"{State.GetDescription()}{_fullName}";
            if (IsEquipped.IsSome(out var equippedNow))
            {
                info += equippedNow
                    ? $" ({ItemDescriptionRichText.RichMeta("装備中")})"
                    : $" ({ItemDescriptionRichText.RichMeta("未装備")})";
            }
            else if (MaxUsages > 1)
            {
                info += $" ({ItemDescriptionRichText.RichMeta(_remainingUsages.CurrentValue)}/{ItemDescriptionRichText.RichMeta(MaxUsages)})";
            }

            info += "\n";
            if (UpgradeCount > 0)
                info += $"それは{ItemDescriptionRichText.RichMeta(UpgradeCount)}/{ItemDescriptionRichText.RichMeta(UpgradeLimit)}回強化されている\n";
            info += CursedInfo();
            info += BuildActivatableSkillSection(useActivatableSkillTemplate);

            info += "\n";

            if (UseOnDeath)
            {
                info += "それは死亡時に自動的に使用される\n";
            }

            if (UsageLossChance == 0)
            {
                info += "それは使用可能回数が減少しない\n";
            }
            else if (UsageLossChance < 1)
            {
                info += ItemDescriptionRichText.ColorPercentagesInPlainText(
                    $"それは{(1 - UsageLossChance):P0}の確率で使用可能回数が減少しない\n");
            }

            foreach (var condition in PassiveConditions)
            {
                info += $"それは{ItemDescriptionRichText.RichPassiveConditionName(condition.Name)}の効果を授ける\n";
            }

            info += FullInfoImpl();

            return info;
        }

        private string BuildActivatableSkillSection(bool useTemplateWhenAvailable)
        {
            if (!HasActivatableSkill)
                return "";

            if (useTemplateWhenAvailable)
            {
                var templated = BuildTemplatedActivatableSkillInfo();
                if (templated != null)
                    return templated;
            }

            if (HasSameSkill)
            {
                var info = "\n" + ItemDescriptionRichText.HeaderLine("使用または投擲したときの効果...") + "\n" + SkillOnUse.Expect("SkillOnUse is null").Skill.Match(
                    spawnEffectSkill => spawnEffectSkill.InfoOnUse(omitProbabilityOfSuccess: true, useOrThrowCombinedTargets: true) + "\n",
                    itemTargetSkill => throw new Exception("SkillOnUse can not be ItemTargetSkill"),
                    inventoryTargetSkill => throw new Exception("SkillOnUse can not be InventoryTargetSkill"),
                    equipToggleSkill => equipToggleSkill.Info()
                );
                var skillOnUseSuccessProbability = SkillOnUse.Expect("SkillOnUse is null").Skill.Match(
                    spawnEffectSkill => spawnEffectSkill.ProbabilityOfSuccess,
                    itemTargetSkill => throw new Exception("SkillOnUse can not be ItemTargetSkill"),
                    inventoryTargetSkill => throw new Exception("SkillOnUse can not be InventoryTargetSkill"),
                    _ => 1f
                );
                var skillOnThrowSuccessProbability = SkillOnThrow.Expect("SkillOnThrow is null").Skill.Match(
                    spawnEffectSkill => spawnEffectSkill.ProbabilityOfSuccess,
                    itemTargetSkill => throw new Exception("SkillOnThrow can not be ItemTargetSkill"),
                    inventoryTargetSkill => throw new Exception("SkillOnThrow can not be InventoryTargetSkill"),
                    _ => 1f
                );
                info += ItemDescriptionRichText.ColorPercentagesInPlainText(
                    $"成功率：使用{skillOnUseSuccessProbability:P0}／投擲{skillOnThrowSuccessProbability:P0}\n");
                return info;
            }

            var generic = SkillOnUse.MapOr(
                "",
                skill => "\n" + ItemDescriptionRichText.HeaderLine("使用したときの効果...") + "\n" + skill.Info()
            );

            generic += SkillOnThrow.MapOr(
                "",
                skill => "\n" + ItemDescriptionRichText.HeaderLine("投擲したときの効果...") + "\n" + skill.Skill.Match(
                    spawnEffectSkill => spawnEffectSkill.InfoOnThrow(HasSameEffect),
                    itemTargetSkill => throw new Exception("SkillOnThrow can not be ItemTargetSkill"),
                    inventoryTargetSkill => throw new Exception("SkillOnThrow can not be InventoryTargetSkill"),
                    equipToggleSkill => equipToggleSkill.Info()
                )
            );
            return generic;
        }
        #endregion

        public bool Equals(IItem other)
        {
            return other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.Value.GetHashCode();
        }
    }
}