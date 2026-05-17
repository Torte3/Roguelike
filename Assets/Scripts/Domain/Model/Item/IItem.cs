#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;
using Domain.Model.Item;

namespace Domain.Model.Item
{
    public interface IItem : IEquatable<IItem>
    {
        public Id<IItem> Id { get; }
        public string BaseName { get; }
        public ItemCategory Category { get; }
        public string UnknownName(ItemPlaceholders itemPlaceholders);
        public string RevealedName { get; }
        public Option<string> CustomName { get; }
        public string DebugName { get; }
        public string GetName(IPlayer player, ItemPlaceholders itemPlaceholders);
        public Sprite Icon { get; }
        public bool IsShiny { get; }
        public ItemState State { get; }
        public bool UseOnDeath { get; }
        public int GetPrice(ItemMarketPriceTable market);
        public bool HasActivatableSkillWhenUsed { get; }
        public bool HasActivatableSkillWhenThrown { get; }
        public bool CanActivateWhenUsed { get; }
        public bool CanActivateWhenThrown { get; }
        public Option<ISkillWithCost> SkillOnUse { get; }
        public Option<ISkillWithCost> SkillOnThrow { get; }
        public bool HasActivatableSkill { get; }
        public bool CanActivate { get; }
        public bool CanAttemptUse { get; }
        public bool CanAttemptThrow { get; }
        public bool CanAttemptUseOrThrow { get; }
        public float EvaluateWhenUsed(IActor actor, Vector2Int position, Direction8 direction, IMap map);
        public float EvaluateWhenThrown(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map);
        public Option<bool> IsEquipped { get; }
        public int MaxUsages { get; }
        public ReadOnlyReactiveProperty<int> RemainingUses { get; }
        public ReadOnlyReactiveProperty<bool> Cursed { get; }
        public bool IsCursed { get; }
        public ReadOnlyReactiveProperty<bool> CurseIdentified { get; }
        public bool IsDiscardBlocked { get; }
        public ReadOnlyReactiveProperty<bool> IsPassiveActive { get; }
        public bool RequiresLiteracy { get; }
        public bool IdentifyIfGot { get; }
        public bool IdentifyIfUsed { get; }
        public bool IsCurseIdentified { get; }
        public bool IsInfoIdentified(IPlayer player);
        public bool AutoDestroyWhenDisabled { get; }
        public int UpgradeCount { get; }
        public IReadOnlyList<IConditionData> PassiveConditions { get; }
        public Observable<Unit> OnItemUpdated { get; }
        public Observable<Unit> OnMimicRevealed { get; }
        public void SetState(ItemState state);
        public bool ShouldRevealMimic(IActorOfEffect actor, Vector2Int position, IMap map);
        public UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map);

        public void LogWhyCannotActivateWhenUsed(IActor actor, IMap map);

        public UniTask<ISkillResult> UseWhenThrown(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map);

        public void UpdateTurn();

        public void Repair(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders);
        public void SetCursed(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool isCursed);
        public void SetCurseIdentified(bool isCurseIdentified, IPlayer? logPlayer = null,
            IEntity? logVisibleEntity = null, ItemPlaceholders? logPlaceholders = null);
        public void Rename(string name);
        public void RevertToDefaultName();
        public bool CanUpgrade();
        public bool CanDowngrade();
        public void Upgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool log = true);
        public void Downgrade(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders, bool log = true);
        public string Info(IPlayer player, ItemPlaceholders itemPlaceholders);
        public string DebugInfo();
    }
}