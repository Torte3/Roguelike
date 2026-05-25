#nullable enable
using Domain.Model.Setting;
using Domain.Service.Logs;
using R3;
using VContainer;
using View.UI;

namespace Provider
{
    public class LogPresenter
    {
        private CompositeDisposable _disposables = new();

        [Inject]
        public LogPresenter(LogView logView)
        {
            Settings.GlobalSettings.LogShownMilliSeconds.Value.Subscribe(logView.SetLogShownMilliSeconds).AddTo(_disposables);
            GameLog.OnLogOutput.Subscribe(entry => logView.AddLog(entry.Message, entry.AppendToPrevious)).AddTo(_disposables);
            GameLog.OnClear.Subscribe(_ => logView.Clear()).AddTo(_disposables);
        }

        ~LogPresenter()
        {
            _disposables.Dispose();
        }
    }
}