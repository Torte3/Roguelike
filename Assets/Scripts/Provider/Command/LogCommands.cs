#nullable enable
using System;
using Domain.Model.WorldEvents;
using Game;
using IngameDebugConsole;
using Unity.Logging;
using Unity.Logging.Sinks;
using Logger = Unity.Logging.Logger;

namespace Provider
{
    public class LogCommands
    {
        private readonly World _world;

        public LogCommands(World world)
        {
            _world = world;
            DebugLogConsole.AddCommandInstance(
                "log",
                "画面にログを出力します。",
                "AddLog",
                this);
            DebugLogConsole.AddCommandInstance(
                "setLogLevel",
                "ログレベルを設定します。",
                "SetLogLevel",
                this);
        }

        private void AddLog(string log)
        {
            _world.Events.Record(new DebugMessage(log));
        }

        private void SetLogLevel(LogLevel level)
        {
            try
            {
                Log.Logger = new Logger(
                    new LoggerConfig()
                        .SyncMode.FullSync()
                        .WriteTo.UnityDebugLog(
                            minLevel: level,
                            captureStackTrace: true)
                );
                Log.Info($"LogLevelを{level}に設定しました。");
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}