#nullable enable
using Domain.Model.WorldEvents;
using UnityEngine;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations
{
    internal static class Obtained
    {
        private static readonly Vector2 PopupOffset = new(0f, 0.9f);

        public static ViewOp Popup(ObtainedItem obtained) => new PopupIcon(obtained.Icon, obtained.Position + PopupOffset);
    }
}
