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
using Domain.Model.WorldEvents;
using Domain.Service.Effect;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using Utilities.Result;
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
        private readonly int _additionalPrice;
        private readonly float _multiplyPrice;
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

        public string DebugName => RevealedName;
        public int GetPrice(ItemMarketPriceTable market) => Mathf.RoundToInt(EvaluatePrice(market));
        public bool HasActivatableSkillWhenUsed => SkillOnUse.HasValue;
        public bool HasActivatableSkillWhenThrown => SkillOnThrow.HasValue;
        public abstract bool CanActivateWhenUsed { get; }
        public abstract bool CanActivateWhenThrown { get; }
        public bool HasActivatableSkill => HasActivatableSkillWhenUsed || HasActivatableSkillWhenThrown;
        public bool CanActivate => CanActivateWhenUsed || CanActivateWhenThrown;
        public virtual bool CanBeMergeBase => true;
        public virtual IReadOnlyList<ItemFeature>? FeaturesForWeaponMerge => null;
        public virtual IReadOnlyList<ArtifactPassiveConditionBundle>? PassiveSlotsForEquipmentMerge => null;
        public abstract bool CanAcceptMergeMaterial(IItem material);
        public abstract IItem MergeWith(IItem material);

        public abstract bool CanAttemptUse { get; }

        public abstract bool CanAttemptThrow { get; }

        public bool CanAttemptUseOrThrow => CanAttemptUse || CanAttemptThrow;

        public abstract bool IsDiscardBlocked { get; }
        public abstract Option<bool> IsEquipped { get; }

        public ItemUseKind UseKind => IsEquipped.MapOr(ItemUseKind.Use,
            isEquipped => isEquipped ? ItemUseKind.Unequip : ItemUseKind.Equip);
        public abstract ReadOnlyReactiveProperty<bool> IsPassiveActive { get; }
        public ReadOnlyReactiveProperty<int> RemainingUses => _remainingUsages;
        public IReadOnlyList<IConditionData> PassiveConditions => _conditions;
        public Observable<Unit> OnMimicRevealed => _onMimicRevealed;

        public ItemName GetName(IItemKnowledge knowledge, IReadOnlyItemPlaceholders itemPlaceholders)
        {
            var isIdentified = knowledge.IsKnownItem(this);
            return NameOf(isIdentified, isIdentified ? null : itemPlaceholders.GetPlaceholder(BaseName, Category));
        }

        private ItemName NameOf(bool isIdentified, string? placeholder)
        {
            return new ItemName(isIdentified, CustomName.IsSome(out var customName) ? customName : null,
                isIdentified ? RevealedName : null, placeholder, isIdentified ? UpgradeCount : 0);
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

        private protected BaseItemMemento SerializeBase()
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

        private protected static BaseItemMemento BuildBase(
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
        }

        private protected void RecordChange(IEntity holder, IMap map, ItemChangeKind kind)
        {
            RecordChange(holder, map, kind, this.NameIn(map));
        }

        private protected void RecordChange(IEntity holder, IMap map, ItemChangeKind kind, ItemName name)
        {
            map.Events.Record(new ItemChanged(holder.Entity.IsVisible, name, kind, this.LookIn(map), holder.HeldItemsIn(map, this),
                holder.UnderfootIn(map), map.ShopLookIn()));
        }

        public bool ShouldRevealMimic(IActorOfEffect actor, Vector2Int position, IMap map)
        {
            if (_mimic.IsSome(out var mimic))
            {
                var label = this.LabelIn(map);
                _onMimicRevealed.OnNext(Unit.Default);
                map.Events.Record(new MimicRevealed(actor.Entity.IsVisible, label, mimic.Name, actor.HeldItemsIn(map)));
                map.SpawnEnemyIgnoreMimic(mimic, position, doActImmediately: true, isSlept: false, isShiny: false);
                return true;
            }
            return false;
        }

        public async UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            if (!PassesChecksBeforeUse(actor, actor, position, map))
            {
                return SkillOutcome.Failed;
            }

            var skill = SkillOnUse.Expect("SkillOnUse is null");

            if (!skill.IsUsable())
            {
                map.Events.Record(new SkillFailed(actor.Entity.IsVisible, SkillFailureKind.Fizzled, false));
                OnUseAttemptedInHand(actor, map);
                return SkillOutcome.Failed;
            }

            var preparation = await skill.Prepare(actor, this, position, direction, map);
            if (!preparation.IsOk(out var execution))
            {
                return SkillOutcome.NotRun(preparation);
            }

            var result = await Run(actor, map, execution);
            OnUseAttemptedInHand(actor, map);
            return result;
        }

        public abstract Result<Unit, ItemActionFailure> ActivationCheckWhenUsed();

        public async UniTask<ISkillResult> UseWhenThrown(IActorOfEffect actor, Vector2Int position,
            Direction8 direction, IMap map)
        {
            if (!PassesChecksBeforeUse(actor, actor as IHasInventory, position, map))
            {
                return SkillOutcome.Failed;
            }

            if (!SkillOnThrow.IsSome(out var skill) || !skill.IsUsable())
            {
                return SkillOutcome.Failed;
            }

            return await Run(actor, map, () => SkillExtension.Match(
                skill.Skill,
                spawnEffectSkill => spawnEffectSkill.Use(actor, this, position, direction, map),
                itemTargetSkill => throw new Exception(
                    "The item is not configured to activate this type of skill when thrown."),
                inventoryTargetSkill => throw new Exception(
                    "The item is not configured to activate this type of skill when thrown."),
                equipToggleSkill => equipToggleSkill.Use(actor, this, position, direction, map)
            ));
        }

        private bool PassesChecksBeforeUse(IActorOfEffect actor, IHasInventory? holder, Vector2Int position, IMap map)
        {
            if (ShouldRevealMimic(actor, position, map))
            {
                return false;
            }

            holder?.KnowCurse(this, true);

            if (this.ReadCheck(actor).IsFailed(out var failure))
            {
                map.Events.Record(new ItemActionFailed(actor.Entity.IsVisible, this.NameIn(map), failure));
                return false;
            }

            return true;
        }

        private async UniTask<ISkillResult> Run(IActorOfEffect actor, IMap map, SkillExecution execution)
        {
            var isConsumed = ConfirmUse(actor, map);
            var result = await execution();
            FinishUse(actor, map, isConsumed);
            return result;
        }

        private protected virtual bool ConfirmUse(IActorOfEffect actor, IMap map) => false;

        private protected abstract void FinishUse(IActorOfEffect actor, IMap map, bool isConsumed);

        private protected virtual void OnUseAttemptedInHand(IActorOfEffect actor, IMap map)
        {
        }

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

        public void UpdateTurn(IEntity holder, IMap map)
        {
            if (SkillOnUse.HasValue && !SkillOnUse.Value.IsUsable())
            {
                SkillOnUse.Value.CoolDown();
                if (SkillOnUse.Value.IsUsable())
                {
                    RecordChange(holder, map, ItemChangeKind.Recharged);
                }
            }
            if (SkillOnThrow.HasValue && !SkillOnThrow.Value.IsUsable())
            {
                SkillOnThrow.Value.CoolDown();
                if (SkillOnThrow.Value.IsUsable())
                {
                    RecordChange(holder, map, ItemChangeKind.Recharged);
                }
            }
        }

        public abstract void Repair(IEntity itemHolder, IMap map);

        public void SetCursed(IEntity itemHolder, IMap map, bool isCursed)
        {
            var name = this.NameIn(map);
            if (_isCursed.CurrentValue != isCursed)
            {
                _isCursed.Value = isCursed;
                if (!isCursed)
                {
                    UsedWhileCursed = false;
                }
            }

            SetCurseIdentified(true, itemHolder, map, false);
            RecordChange(itemHolder, map, isCursed ? ItemChangeKind.Cursed : ItemChangeKind.Uncursed, name);
        }

        public void SetCurseIdentified(bool isCurseIdentified, IEntity holder, IMap map, bool log)
        {
            var wasUnidentified = !IsCurseIdentified;
            _isCurseIdentified.Value = isCurseIdentified;
            if (!isCurseIdentified || !wasUnidentified)
                return;

            RecordChange(holder, map, IsCursed && log ? ItemChangeKind.CurseRevealed : ItemChangeKind.CurseIdentified);
        }

        public void Rename(string name)
        {
            CustomName = Option.Some(name);
        }

        public void RevertToDefaultName()
        {
            CustomName = Option.None<string>();
        }

        #region Upgrade

        public abstract bool CanUpgrade();
        public abstract bool CanDowngrade();
        public abstract void Upgrade(IEntity itemHolder, IMap map, bool log = true);
        public abstract void Downgrade(IEntity itemHolder, IMap map, bool log = true);

        #endregion
        #region Info

        private CurseKnowledge Curse => !IsCurseIdentified
            ? CurseKnowledge.Unknown
            : IsCursed ? CurseKnowledge.Cursed : CurseKnowledge.NotCursed;

        public ItemDescription Describe(IItemKnowledge knowledge, IReadOnlyItemPlaceholders itemPlaceholders)
        {
            var name = GetName(knowledge, itemPlaceholders);
            return name.IsIdentified ? DescribeIdentified(name, true) : DescribeUnidentified(name);
        }

        public ItemDescription DescribeIdentified()
        {
            return DescribeIdentified(NameOf(true, null), true);
        }

        public ItemDescription DescribeIdentifiedWithoutSkillTemplate()
        {
            return DescribeIdentified(NameOf(true, null), false);
        }

        private ItemDescription DescribeUnidentified(ItemName name)
        {
            return new ItemDescription(
                State: State,
                Name: name,
                IsEquipped: null,
                RemainingUses: 0,
                MaxUses: 0,
                UpgradeCount: 0,
                UpgradeLimit: 0,
                Curse: Curse,
                CanUse: HasActivatableSkillWhenUsed,
                CanThrow: HasActivatableSkillWhenThrown,
                Details: null,
                Abilities: null);
        }

        private ItemDescription DescribeIdentified(ItemName name, bool useActivatableSkillTemplate)
        {
            return new ItemDescription(
                State,
                name,
                IsEquipped.IsSome(out var isEquipped) ? isEquipped : null,
                _remainingUsages.CurrentValue,
                MaxUsages,
                UpgradeCount,
                UpgradeLimit,
                Curse,
                HasActivatableSkillWhenUsed,
                HasActivatableSkillWhenThrown,
                BuildDetails(useActivatableSkillTemplate),
                Abilities);
        }

        public string DebugInfo()
        {
            return DescribeIdentified().ToString();
        }

        protected abstract ItemAbilities? Abilities { get; }

        /// <summary>識別済み表示で使用する効果の要約。null のときは従来の詳細表示にフォールバックする。</summary>
        protected virtual string? BuildTemplatedActivatableSkillInfo() => null;

        /// <summary>インスペクタでのプレビュー用。</summary>
        public string PreviewTemplatedSkillSection() => BuildTemplatedActivatableSkillInfo() ?? "";

        private string BuildDetails(bool useActivatableSkillTemplate)
        {
            var description = BuildActivatableSkillSection(useActivatableSkillTemplate);

            description += "\n";

            if (UseOnDeath)
            {
                description += "それは死亡時に自動的に使用される\n";
            }

            if (UsageLossChance == 0)
            {
                description += "それは使用可能回数が減少しない\n";
            }
            else if (UsageLossChance < 1)
            {
                description += ItemDescriptionRichText.ColorPercentagesInPlainText(
                    $"それは{(1 - UsageLossChance):P0}の確率で使用可能回数が減少しない\n");
            }

            foreach (var condition in PassiveConditions)
            {
                description += $"それは{ItemDescriptionRichText.RichPassiveConditionName(condition.Name)}の効果を授ける\n";
            }

            return description;
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
                var description = "\n" + ItemDescriptionRichText.HeaderLine("使用または投擲したときの効果...") + "\n" + SkillOnUse.Expect("SkillOnUse is null").Skill.Match(
                    spawnEffectSkill => spawnEffectSkill.DescriptionOnUse(omitProbabilityOfSuccess: true, useOrThrowCombinedTargets: true) + "\n",
                    itemTargetSkill => throw new Exception("SkillOnUse can not be ItemTargetSkill"),
                    inventoryTargetSkill => throw new Exception("SkillOnUse can not be InventoryTargetSkill"),
                    equipToggleSkill => equipToggleSkill.Description()
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
                description += ItemDescriptionRichText.ColorPercentagesInPlainText(
                    $"成功率：使用{skillOnUseSuccessProbability:P0}／投擲{skillOnThrowSuccessProbability:P0}\n");
                return description;
            }

            var generic = SkillOnUse.MapOr(
                "",
                skill => "\n" + ItemDescriptionRichText.HeaderLine("使用したときの効果...") + "\n" + skill.Description()
            );

            generic += SkillOnThrow.MapOr(
                "",
                skill => "\n" + ItemDescriptionRichText.HeaderLine("投擲したときの効果...") + "\n" + skill.Skill.Match(
                    spawnEffectSkill => spawnEffectSkill.DescriptionOnThrow(HasSameEffect),
                    itemTargetSkill => throw new Exception("SkillOnThrow can not be ItemTargetSkill"),
                    inventoryTargetSkill => throw new Exception("SkillOnThrow can not be InventoryTargetSkill"),
                    equipToggleSkill => equipToggleSkill.Description()
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