using Domain.Model.Character;

namespace Domain.Model
{
    public interface IHasName
    {
        public string GetName(IPlayer player);
        public string GetNameIgnoreVisibility(IPlayer player);
    }
}