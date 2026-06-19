#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Service.Events;
using Provider.Input;
using R3;
using VContainer;
using View.UI;

namespace Provider
{
    public class TutorialPresenter
    {
        [Inject]
        public TutorialPresenter(TutorialReceiver tutorialReceiver, InputReceiver inputReceiver,
            TutorialWindow tutorialWindow, MenuController menuController, IGameManager gameManager)
        {
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
                    gameManager.PlaySE(SE.ChoiceConfirm);
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
