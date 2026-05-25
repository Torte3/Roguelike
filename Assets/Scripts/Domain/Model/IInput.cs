namespace Domain.Model
{
    public interface IInput
    {
        public bool IsDash();
        public bool IsNoMove();
        public bool IsDiagonalOnly();
    }
}