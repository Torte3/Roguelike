#nullable enable
using System;
using Domain.Model.Character.Behavior;

namespace Domain.Service.Characters.Behavior
{
    internal sealed record MoveRule(MoveTypeWhenDiscoveringTarget Type, IMovement Movement)
    {
        public static MoveRule Of(MoveTypeWhenDiscoveringTarget type)
        {
            return new MoveRule(type, type switch
            {
                MoveTypeWhenDiscoveringTarget.Chase => new Chase(),
                MoveTypeWhenDiscoveringTarget.Escape => new Escape(),
                MoveTypeWhenDiscoveringTarget.Wander => new Wander(),
                MoveTypeWhenDiscoveringTarget.NoMove => new NoMove(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            });
        }
    }
}
