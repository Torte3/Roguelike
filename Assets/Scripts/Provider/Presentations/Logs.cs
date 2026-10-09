#nullable enable
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations
{
    internal static class Logs
    {
        public static ViewOp Line(string text) => new AddLog(text, false);

        public static ViewOp Appended(string text) => new AddLog(text, true);

        public static ViewOp Placed(bool appendsToPrevious, string text) => new AddLog(text, appendsToPrevious);
    }
}
