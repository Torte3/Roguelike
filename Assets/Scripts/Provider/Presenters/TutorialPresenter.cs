#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Service.Events.Receivers;
using Provider.Input;
using R3;
using VContainer;
using View.Audio;
using View.Playback;
using View.UI;
using View.UI.Menus;

namespace Provider.Presenters
{
    public class TutorialPresenter
    {
        [Inject]
        public TutorialPresenter(TutorialReceiver tutorialReceiver, InputReceiver inputReceiver,
            TutorialWindow tutorialWindow, MenuController menuController, SEManager seManager,
            PlaybackQueue playback)
        {
            tutorialReceiver.SetReadyWaiter(playback.WaitUntilIdle);
            var isShowing = false;
            tutorialWindow.Hide();

            tutorialReceiver.OnShown.Subscribe(type =>
            {
                isShowing = true;
                tutorialWindow.SetContent((int)type);
                menuController.AddMenu(tutorialWindow);
            });
            tutorialReceiver.OnHidden.Subscribe(_ =>
            {
                isShowing = false;
                menuController.PopMenu();
            });

            inputReceiver.OnSubmitPerformed
                .Where(_ => isShowing)
                .Subscribe(_ =>
                {
                    isShowing = false;
                    seManager.ChoiceConfirmSE();
                    AdvanceNextFrame(tutorialReceiver).Forget();
                });
        }

        private static async UniTaskVoid AdvanceNextFrame(TutorialReceiver tutorialReceiver)
        {
            await UniTask.Yield();
            tutorialReceiver.Advance();
        }
    }
}
