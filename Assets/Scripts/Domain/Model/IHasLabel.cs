using Domain.Model.Character;

namespace Domain.Model
{
    public interface IHasLabel
    {
        public CharacterLabel Label { get; }
    }
}
