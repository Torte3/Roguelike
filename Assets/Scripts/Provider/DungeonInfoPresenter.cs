#nullable enable
using Game;
using VContainer;
using View.UI;
using R3;

namespace Provider
{
    public class DungeonInfoPresenter
    {
        [Inject]
        public DungeonInfoPresenter(World world, DungeonInfoView dungeonInfoView)
        {
            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                var map = mapChanged.Map;
                dungeonInfoView.SetInfo(map.Name, map.Depth);
            });
        }
    }
}