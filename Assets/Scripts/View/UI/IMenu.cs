namespace View.UI
{
    public interface IMenu
    {
        public bool CanClose { get; }
        public void Show();
        public void Hide();
        public void Enable();
        public void Disable();
    }
}