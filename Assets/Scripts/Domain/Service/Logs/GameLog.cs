using R3;
using Unity.Logging;

namespace Domain.Service.Logs
{
    public static class GameLog
    {
        public static Observable<GameLogEntry> OnLogOutput => _onLogOutput;
        public static Observable<Unit> OnClear => _onClear;
        private static readonly Subject<GameLogEntry> _onLogOutput = new();
        private static readonly Subject<Unit> _onClear = new();

        public static void AddIgnoreVisibility(string log)
        {
            _onLogOutput.OnNext(GameLogEntry.NewLine(log));
            Log.Info($"[GameLog]{log}");
        }

        public static void Add(bool isVisible, string log)
        {
            if (isVisible)
            {
                _onLogOutput.OnNext(GameLogEntry.NewLine(log));
                Log.Info($"[GameLog]{log}");
            }
            else
            {
                Log.Info($"[GameLog](Hidden){log}");
            }
        }

        public static void AddAppend(bool isVisible, string log)
        {
            if (isVisible)
            {
                _onLogOutput.OnNext(GameLogEntry.Append(log));
                Log.Info($"[GameLog]{log}");
            }
            else
            {
                Log.Info($"[GameLog](Hidden){log}");
            }
        }

        public static void Clear()
        {
            _onClear.OnNext(Unit.Default);
        }
    }

    public readonly struct GameLogEntry
    {
        public string Message { get; }
        public bool AppendToPrevious { get; }

        private GameLogEntry(string message, bool appendToPrevious)
        {
            Message = message;
            AppendToPrevious = appendToPrevious;
        }

        public static GameLogEntry NewLine(string message)
        {
            return new GameLogEntry(message, false);
        }

        public static GameLogEntry Append(string message)
        {
            return new GameLogEntry(message, true);
        }
    }
}