using Game;
using R3;
using UnityEngine;
using VContainer;
using View;

namespace Provider
{
    public class PickupEmphasisPresenter
    {
        [Inject]
        public PickupEmphasisPresenter(World world, GameManager gameManager)
        {
            gameManager.OnWorldIconPopupRequested.Subscribe(message =>
            {
                var map = world.CurrentMap;
                if (map == null)
                    return;
                if (!map.Player.Character.IsVisible(message.Position))
                    return;

                WorldIconPopup.Show(message.Icon, message.Position + new Vector2(0f, 0.9f));
            });
        }
    }
}
