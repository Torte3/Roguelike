#nullable enable
using System;
using Domain.Service.Logs;
using IngameDebugConsole;
using Unity.Logging;
using Unity.Logging.Sinks;
using Logger = Unity.Logging.Logger;

namespace Provider
{
    public class LogCommands
    {
        public LogCommands()
        {
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
            GameLog.AddIgnoreVisibility(log);
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