#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Effect;
using UnityEngine;
using Utilities;

namespace Domain.Model.WorldEvents
{
    public record SkillUsed(EntityRef Entity, SkillSource Source) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;
    public record EffectHit(EntityRef Target, Impact Impact);
    public record StatueStruck(EntityRef Entity) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;

    public enum SkillFailureKind
    {
        NoEffect,
        ItemUnaffected,
        Fizzled,
    }

    public record SkillFailed(bool IsVisible, SkillFailureKind Kind, bool FollowsActivation) : WorldEvent(IsVisible);
    public record SkillPartiallySucceeded(bool IsVisible, int Successes, bool FollowsActivation) : WorldEvent(IsVisible);
    public record ConditionInflicted(EntityRef Entity, CharacterLabel Label, string? ConditionLog, IReadOnlyList<ParticleType> Particles, ShopLook? Shop)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IShopEvent;
    public record ConditionRemoved(EntityRef Entity, CharacterLabel Label, string? ConditionLog, IReadOnlyList<ParticleType> Particles, ShopLook? Shop)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IShopEvent;
    public record ConditionResisted(EntityRef Entity, CharacterLabel Label, string ConditionName) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;
    public record EntityBroken(bool IsVisible, EntityLabel Target) : WorldEvent(IsVisible);

    public enum EffectMissReason
    {
        CannotBeCursed,
        NoItemToCurse,
        DidNotDropItem,
        HasNoItem,
        NoUpgradedItem,
    }

    public record EffectMissed(EntityRef Entity, CharacterLabel Label, EffectMissReason Reason) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;

    public enum MemoryKind
    {
        ItemNames,
        Characters,
    }

    public record MemoryLost(EntityRef Entity, CharacterLabel Label, MemoryKind Kind, InventoryLook? Inventory, Underfoot? Underfoot)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IInventoryEvent, IUnderfootEvent;

    public record Flight(Sprite Icon, Vector2Int From, Vector2Int To);

    public interface IFlightEvent
    {
        public Flight Flight { get; }
    }

    public record ProjectileFlew(bool IsVisible, Flight Flight) : WorldEvent(IsVisible), IFlightEvent;
}
