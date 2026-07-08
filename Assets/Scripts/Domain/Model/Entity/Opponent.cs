#nullable enable
using Domain.Model.Character;
using Domain.Model.Effect;

namespace Domain.Model.Entity
{
    public record Opponent(CharacterLabel Label, bool IsVisible)
    {
        public static Opponent? Of(IActorOfEffect? actor)
        {
            return actor == null ? null : new Opponent(actor.Label, actor.Entity.IsVisible);
        }
    }
}
