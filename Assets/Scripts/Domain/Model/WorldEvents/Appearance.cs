#nullable enable
using Domain.Model.Entity;
using UnityEngine;

namespace Domain.Model.WorldEvents
{
    public record Appearance(
        EntityKind Kind,
        EntityLayer Layer,
        Sprite? Icon,
        bool IsShiny);
}
