#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Service.Events;
using Provider.Input;
using R3;
using VContainer;
using View;
using View.Playback;
using View.UI;

namespace Provider
{
    // チュートリアル表示の配線。表示通知でウィンドウをメニュースタックに載せ（＝メニュー入力モードへ）、
    // 決定キー（UI.Submit）で選択音を鳴らして次へ進める。入力の扱いは入力層に閉じ、View は表示のみ。
    public class TutorialPresenter
    {
        [Inject]
        public TutorialPresenter(TutorialReceiver tutorialReceiver, InputReceiver inputReceiver,
            TutorialWindow tutorialWindow, MenuController menuController, SEManager seManager,
            PlaybackQueue playback)
        {
            var isShowing = false;
            // 初回以外（表示済み）でも出っぱなしにならないよう、起動時は必ず隠す。
            tutorialWindow.Hide();

            tutorialReceiver.OnShown.Subscribe(async type =>
            {
                await playback.WaitUntilIdle();
                isShowing = true;
                tutorialWindow.SetContent((int)type);
                // メニューに載せることでフィールド中でもメニュー入力モード（UI有効）になり、決定キーが効く。
                menuController.AddMenu(tutorialWindow);
            });
            tutorialReceiver.OnHidden.Subscribe(_ =>
            {
                isShowing = false;
                menuController.PopMenu();
            });

            // ボタンを持たないため、決定キーで次へ進める（表示中のみ）。選択音も鳴らす。
            inputReceiver.OnSubmitPerformed
                .Where(_ => isShowing)
                .Subscribe(_ =>
                {
                    isShowing = false;
                    seManager.ChoiceConfirmSE();
                    // 入力(UI.Submit)コールバック内で Advance→PopMenu→SwitchField と進むと、
                    // コールバック実行中に UI アクションマップを無効化してしまい IndexOutOfRange になる。
                    // 次フレームへ遅延してコールバックの外で閉じる。
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
