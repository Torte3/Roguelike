#nullable enable
using Game;
using R3;
using UnityEngine;
using Utilities;
using VContainer;

namespace Provider
{
    public class PlayerCameraPresenter
    {
        [Inject]
        public PlayerCameraPresenter(World world, SynchronizedCharacterView characters,
            CameraFollowTarget targetCamera, CameraFlameRect rectCamera)
        {
            var disposable = new SerialDisposable();
            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                var map = mapChanged.Map;
                if (map.Player.Character.IsDead)
                {
                    targetCamera.SetPosition((Vector3Int)map.Player.Character.Entity.CurrentPosition);
                    return;
                }

                var playerView = characters.Get(map.Player.Character);
                targetCamera.SetTarget(playerView.gameObject);
                disposable.Disposable = map.TilemapViewer.Rect.Subscribe(rect => rectCamera.SetRect(rect));
            });
        }
    }
}