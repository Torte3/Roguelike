#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Skill;
using Domain.Model.Entity;
using Domain.Model.Item.Data;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Result;
using Utilities.Serialize.Option;

namespace Domain.Model.Item
{
    public interface IItem : IReadOnlyItem, IEquatable<IItem>
    {
        public ItemCategory Category { get; }
        public Option<string> CustomName { get; }
        public string DebugName { get; }
        public bool UseOnDeath { get; }
        public bool HasActivatableSkillWhenUsed { get; }
        public bool HasActivatableSkillWhenThrown { get; }
        public bool CanActivateWhenUsed { get; }
        public bool CanActivateWhenThrown { get; }
        public bool CanBeMergeBase { get; }
        public bool CanBeMergedInto(IWeaponMergeBase weapon);
        public IItem MergeInto(IWeaponMergeBase weapon);
        public bool CanBeMergedInto(IEquipmentMergeBase equipment);
        public IItem MergeInto(IEquipmentMergeBase equipment);
        public bool CanAcceptMergeMaterial(IItem material);
        public IItem MergeWith(IItem material);
        public Option<ISkillWithCost> SkillOnUse { get; }
        public Option<ISkillWithCost> SkillOnThrow { get; }
        public bool CanActivate { get; }
        public bool CanAttemptUse { get; }
        public bool CanAttemptThrow { get; }
        public float EvaluateWhenUsed(IActor actor, Vector2Int position, Direction8 direction, IMap map);
        public float EvaluateWhenThrown(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map);
        public EquipState EquipState { get; }
        public int MaxUsages { get; }
        public ReadOnlyReactiveProperty<bool> Cursed { get; }
        public ReadOnlyReactiveProperty<bool> CurseIdentified { get; }
        public bool IsDiscardBlocked { get; }
        public ReadOnlyReactiveProperty<bool> IsPassiveActive { get; }
        public bool RequiresLiteracy { get; }
        public bool IdentifyIfGot { get; }
        public bool IdentifyIfUsed { get; }
        public bool AutoDestroyWhenDisabled { get; }
        public int UpgradeCount { get; }
        public IReadOnlyList<IConditionData> PassiveConditions { get; }
        public Observable<Unit> OnMimicRevealed { get; }
        public void SetState(ItemState state);
        public bool ShouldRevealMimic(IActorOfEffect actor, Vector2Int position, IMap map);
        public UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map, SkillSource source);

        public Result<Unit, ItemActionFailure> ActivationCheckWhenUsed();
        public Result<Unit, ItemActionFailure> ThrowCheck();
        public Result<Unit, ItemActionFailure> DiscardCheck();
        public Result<Unit, ItemActionFailure> ReadCheck(IActorOfEffect reader);
        public Result<Unit, FacilityFailure> PutInCheck();

        public UniTask<ISkillResult> UseWhenThrown(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map, SkillSource source);

        public void UpdateTurn(IEntity holder, IMap map);

        public void Repair(IEntity itemHolder, IMap map);
        public void SetCursed(IEntity itemHolder, IMap map, bool isCursed);
        public void SetCurseIdentified(bool isCurseIdentified, IEntity holder, IMap map, DiscoveryCause cause);
        public void Rename(string name);
        public void RevertToDefaultName();
        public bool CanUpgrade();
        public bool CanDowngrade();
        public void Upgrade(IEntity itemHolder, IMap map);
        public void Downgrade(IEntity itemHolder, IMap map);
        public IReadOnlyItem PreviewUpgrade();
        public string DebugInfo();
    }
}