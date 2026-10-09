#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Entity;
using UnityEngine;
using Utilities;

namespace Domain.Model.WorldEvents
{
    public interface IEntityWorldEvent
    {
        public EntityRef Entity { get; }
    }

    public interface IHealthEvent : IEntityWorldEvent
    {
        public CharacterLabel Label { get; }
        public Health Health { get; }
    }

    public enum MoveKind
    {
        Walk,
        Thrown,
        Teleport,
    }

    public record EntityAppeared(EntityRef Entity, Appearance Appearance, Underfoot? Underfoot = null)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IUnderfootEvent;
    public record FacilityAppeared(EntityRef Entity, Appearance Appearance, bool IsUsable)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent;
    public record ChestAppeared(EntityRef Entity, Appearance Appearance, int LockCount, bool CanOpen)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent;

    public record CharacterAppeared(
        EntityRef Entity,
        Appearance Appearance,
        CharacterLooks Looks,
        CharacterLabel Label,
        Direction8 Direction,
        Health Health,
        IReadOnlyList<ParticleType> Particles,
        ChargeState? Charge,
        bool IsKeyHolder,
        bool AppearsInteractable)
        : WorldEvent(Entity.IsVisible), IHealthEvent;

    public record EntityDisappeared(EntityRef Entity, Underfoot? Underfoot)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IUnderfootEvent;
    public record EntityMoved(EntityRef Entity, MoveKind Kind, bool WasVisible, bool IsPlayer, Underfoot? Underfoot)
        : WorldEvent(WasVisible || Entity.IsVisible), IEntityWorldEvent, IUnderfootEvent;
    public record VisibilityChanged(EntityRef Entity) : AlwaysVisibleEvent, IEntityWorldEvent;
    public record DirectionChanged(EntityRef Entity, Direction8 Direction) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;
    public record MaxHpChanged(EntityRef Entity, CharacterLabel Label, Health Health) : WorldEvent(Entity.IsVisible), IHealthEvent;
    public record ChargeStarted(EntityRef Entity, ChargeState? Charge) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;
    public record ChargeCountedDown(EntityRef Entity, int Turns) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;
    public record ChargeEnded(EntityRef Entity) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;
    public record AffiliationChanged(EntityRef Entity, AffiliationType Affiliation, bool AppearsInteractable)
        : WorldEvent(Entity.IsVisible), IEntityWorldEvent;
    public record FacilityExhausted(EntityRef Entity, Sprite? Icon) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;
    public record ChestLockReleased(EntityRef Entity, int LockCount, bool CanOpen) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;
    public record KeyHolderChanged(EntityRef Entity, bool IsKeyHolder) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;

    public record CharacterDamaged(
        EntityRef Entity,
        CharacterLabel Label,
        Health Health,
        int Amount,
        DamageSource Source,
        CharacterLabel? Attacker)
        : WorldEvent(Entity.IsVisible), IHealthEvent;

    public record CharacterHealed(
        EntityRef Entity,
        CharacterLabel Label,
        Health Health,
        int Amount,
        int Restored,
        HealCause Cause)
        : WorldEvent(Entity.IsVisible), IHealthEvent;

    public record CharacterDied(EntityRef Entity, CharacterLabel Label, DamageSource Source) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;
    public record CharacterBroken(EntityRef Entity, CharacterLabel Label, ShopLook? Shop) : WorldEvent(Entity.IsVisible), IEntityWorldEvent, IShopEvent;
    public record CharacterTurnSkipped(EntityRef Entity, CharacterLabel Label) : WorldEvent(Entity.IsVisible), IEntityWorldEvent;
}
