#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Character.Affiliation;
using Domain.Model.Entity;
using UnityEngine;
using Utilities;

namespace Domain.Model.WorldEvents
{
    public enum MoveKind
    {
        Walk,
        Thrown,
        Teleport,
    }

    public record EntityAppeared(EntityRef Entity, Appearance Appearance)
        : WorldEvent(Entity.IsVisible);
    public record FacilityAppeared(EntityRef Entity, Appearance Appearance, bool IsUsable)
        : WorldEvent(Entity.IsVisible);
    public record ChestAppeared(EntityRef Entity, Appearance Appearance, int LockCount, bool CanOpen,
        IReadOnlyList<Id<IEntity>> KeyHolders)
        : WorldEvent(Entity.IsVisible);

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
        : WorldEvent(Entity.IsVisible);

    public record EntityDisappeared(EntityRef Entity) : WorldEvent(Entity.IsVisible);
    public record EntityMoved(EntityRef Entity, MoveKind Kind, bool WasVisible)
        : WorldEvent(WasVisible || Entity.IsVisible);
    public record VisibilityChanged(EntityRef Entity) : AlwaysVisibleEvent;
    public record DirectionChanged(EntityRef Entity, Direction8 Direction) : WorldEvent(Entity.IsVisible);
    public record MaxHpChanged(EntityRef Entity, CharacterLabel Label, Health Health) : WorldEvent(Entity.IsVisible);
    public record ChargeStarted(EntityRef Entity, ChargeState? Charge) : WorldEvent(Entity.IsVisible);
    public record ChargeCountedDown(EntityRef Entity, int Turns) : WorldEvent(Entity.IsVisible);
    public record ChargeEnded(EntityRef Entity) : WorldEvent(Entity.IsVisible);
    public record AffiliationChanged(EntityRef Entity, AffiliationType Affiliation, bool AppearsInteractable)
        : WorldEvent(Entity.IsVisible);
    public record FacilityExhausted(EntityRef Entity, Sprite? Icon) : WorldEvent(Entity.IsVisible);
    public record ChestKeyHolderRemoved(EntityRef Entity, int LockCount, bool CanOpen) : WorldEvent(Entity.IsVisible);
    public record InteractableChanged(EntityRef Entity, bool AppearsInteractable) : WorldEvent(Entity.IsVisible);

    public record CharacterDamaged(
        EntityRef Entity,
        CharacterLabel Label,
        Health Health,
        int Amount,
        DamageSource Source,
        Id<IEntity>? Attacker)
        : WorldEvent(Entity.IsVisible);

    public record CharacterHealed(
        EntityRef Entity,
        CharacterLabel Label,
        Health Health,
        int Amount,
        int Restored,
        HealCause Cause)
        : WorldEvent(Entity.IsVisible);

    public record CharacterDied(EntityRef Entity, CharacterLabel Label, DamageSource Source) : WorldEvent(Entity.IsVisible);
    public record CharacterBroken(EntityRef Entity, CharacterLabel Label, DamageSource Source) : WorldEvent(Entity.IsVisible);
    public record CharacterTurnSkipped(EntityRef Entity, CharacterLabel Label) : WorldEvent(Entity.IsVisible);
}
