#nullable enable
using Game;
using IngameDebugConsole;
using Unity.Logging;
using Utilities;
using VContainer;

namespace Provider
{
    public class MapCommands
    {
        private readonly World _world;

        [Inject]
        public MapCommands(World world)
        {
            _world = world;

            DebugLogConsole.AddCommandInstance(
                "getMapId",
                "現在のマップIDを出力します。",
                "GetMapId",
                this);
        }

        private void GetMapId()
        {
            var map = _world.CurrentMap;
            if (map == null)
            {
                Log.Error("マップがロードされていません。");
                return;
            }

            Log.Info($"Current map ID: {map.Id}");
        }
    }
}
