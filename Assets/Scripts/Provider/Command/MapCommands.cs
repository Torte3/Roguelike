#nullable enable
using System;
using Domain.Model.Map;
using Game;
using IngameDebugConsole;
using Unity.Logging;
using Utilities;
using VContainer;

namespace Provider
{
    public class MapCommands
    {
        private readonly GameManager _gameManager;
        private readonly World _world;

        [Inject]
        public MapCommands(GameManager gameManager, World world)
        {
            _gameManager = gameManager;
            _world = world;

            DebugLogConsole.AddCommandInstance(
                "getMapId",
                "現在のマップIDを出力します。",
                "GetMapId",
                this);
            DebugLogConsole.AddCommandInstance(
                "moveMap",
                "指定したマップIDに移動します。",
                "MoveMap",
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

        private void MoveMap(string mapId)
        {
            try
            {
                _gameManager.MoveMap(new Id<IMap>(mapId));
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
