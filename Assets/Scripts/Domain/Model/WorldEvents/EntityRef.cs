#nullable enable
using Domain.Model.Entity;
using UnityEngine;
using Utilities;

namespace Domain.Model.WorldEvents
{
    public record EntityRef(Id<IEntity> Id, Vector2Int Position, bool IsVisible);
}
