#nullable enable

using View.Entities;

namespace View.Playback.Figures
{
    internal sealed class BonfireFigure : InteractiveFigure
    {
        private readonly BonfireView _bonfire;

        public BonfireFigure(EntityView view) : base(view)
        {
            _bonfire = view.GetComponent<BonfireView>();
        }

        public override void SetUsable(bool isUsable)
        {
            _bonfire.ShowFire(isUsable);
        }
    }
}
