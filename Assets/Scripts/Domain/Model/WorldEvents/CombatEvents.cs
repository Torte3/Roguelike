#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.Effect;
using UnityEngine;
using Utilities;

namespace Domain.Model.WorldEvents
{
    public record SkillUsed(EntityRef Entity, SkillSource Source) : WorldEvent(Entity.IsVisible);
    public record EffectHit(EntityRef Target, Impact Impact);
    public record StatueStruck(EntityRef Entity) : WorldEvent(Entity.IsVisible);

    public enum SkillFailureKind
    {
        NoEffect,
        Fizzled,
    }

    public record SkillFailed(bool IsVisible, SkillSource Source, SkillFailureKind Kind) : WorldEvent(IsVisible);
    public record SkillPartiallySucceeded(bool IsVisible, SkillSource Source, int Successes) : WorldEvent(IsVisible);
    public record ConditionInflicted(EntityRef Entity, CharacterLabel Label, string? ConditionLog, bool IsNew, IReadOnlyList<ParticleType> Particles)
        : WorldEvent(Entity.IsVisible);
    public record ConditionRemoved(EntityRef Entity, CharacterLabel Label, string? ConditionLog, bool IsLast, IReadOnlyList<ParticleType> Particles)
        : WorldEvent(Entity.IsVisible);
    public record ConditionResisted(EntityRef Entity, CharacterLabel Label, string ConditionName) : WorldEvent(Entity.IsVisible);
    public record EntityBroken(bool IsVisible, EntityLabel Target) : WorldEvent(IsVisible);

    public enum EffectMissReason
    {
        CannotBeCursed,
        NoItemToCurse,
        DidNotDropItem,
        HasNoItem,
        NoUpgradedItem,
    }

    public record EffectMissed(EntityRef Entity, CharacterLabel Label, EffectMissReason Reason) : WorldEvent(Entity.IsVisible);

    public enum MemoryKind
    {
        ItemNames,
        Characters,
    }

    public record MemoryLost(EntityRef Entity, CharacterLabel Label, MemoryKind Kind)
        : WorldEvent(Entity.IsVisible);

    public record Flight(Sprite Icon, Vector2Int From, Vector2Int To);

    public record ProjectileFlew(bool IsVisible, Flight Flight) : WorldEvent(IsVisible);
}
