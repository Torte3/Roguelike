#nullable enable
using System;
using IngameDebugConsole;
using Unity.Logging;
using Unity.Logging.Sinks;
using View.UI.HUD;
using Logger = Unity.Logging.Logger;

namespace Provider.Command
{
    public class LogCommands
    {
        private readonly LogView _log;

        public LogCommands(LogView log)
        {
            _log = log;
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
            _log.AddLog(log, false);
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