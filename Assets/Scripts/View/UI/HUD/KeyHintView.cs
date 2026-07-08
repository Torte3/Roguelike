#nullable enable
using TMPro;
using UnityEngine;

namespace View.UI.HUD
{
    /// <summary>
    /// 操作説明（キーヒント）を、コントローラー用 / キーボード用のアイコンへ切り替えて表示する。
    /// 入力デバイスの判定は入力層が行い、<see cref="SetUsingKeyboard"/> で切り替える（表示専用）。
    /// </summary>
    public class KeyHintView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _movementHint = null!;
        [SerializeField] private TMP_Text _itemMenuHint = null!;

        private static class Pad
        {
            public const int Dpad = 398;          // 移動方向
            public const int ButtonSouth = 13;    // A 位置
            public const int ButtonEast = 14;     // B 位置
            public const int ButtonWest = 15;     // X 位置
            public const int ButtonNorth = 16;    // Y 位置
            public const int RightShoulder = 586; // RB
            public const int RightTrigger = 584;  // RT
            public const int Start = 652;
            public const int Select = 651;        // Back/Select
            public const int LeftShoulder = 585;  // L（推定）
            public const int LeftStick = 460;     // 左スティック（推定）
        }

        private static class Key
        {
            public const int W = 85;          // 移動 上
            public const int A = 117;          // 移動 左
            public const int S = 118;          // 移動 下
            public const int D = 119;          // 移動 右
            public const int ArrowUp = 162;    // 選択 上
            public const int ArrowDown = 164;  // 選択 下
            public const int ArrowLeft = 165;  // 選択 左
            public const int ArrowRight = 163; // 選択 右
            public const int Shift = 239;    // 走る
            public const int Ctrl = 212;       // 向きのみ変える
            public const int Alt = 183;        // 斜め移動
            public const int X = 152;          // 足踏み
            public const int Tab = 184;        // メニュー
            public const int Space = 149;      // 使う
            public const int E = 86;          // 入れ替え
            public const int Q = 84;          // 投げる
            public const int R = 87;          // 名付ける
        }

        private bool? _isKeyboard;

        /// <summary>使用中の入力デバイスに応じて表示を切り替える（入力層から呼ぶ）。</summary>
        public void SetUsingKeyboard(bool keyboard)
        {
            if (_isKeyboard == keyboard)
                return;
            _isKeyboard = keyboard;

            if (_movementHint != null)
                _movementHint.text = keyboard ? KeyboardMovementHint() : PadMovementHint();
            if (_itemMenuHint != null)
                _itemMenuHint.text = keyboard ? KeyboardItemMenuHint() : PadItemMenuHint();
        }

        private static string I(int index) => $"<sprite={index}>";

        private static string PadMovementHint() =>
            $"{I(Pad.Dpad)}:移動 / {I(Pad.ButtonEast)}+移動:走る / " +
            $"{I(Pad.ButtonNorth)}+移動:向きのみ変える/ {I(Pad.RightShoulder)}+移動:斜め移動 / " +
            $"{I(Pad.ButtonSouth)}+{I(Pad.ButtonEast)}:足踏み / {I(Pad.Start)}:メニュー";

        private static string PadItemMenuHint() =>
            $"アイテムメニュー: {I(Pad.LeftShoulder)}+移動 or {I(Pad.LeftStick)}:選択 / " +
            $"{I(Pad.ButtonSouth)}:使う / {I(Pad.ButtonWest)}:入れ替え、拾う（足元） / {I(Pad.RightTrigger)}:投げる / {I(Pad.Select)}:名付ける";

        private static string Wasd() => I(Key.W) + I(Key.A) + I(Key.S) + I(Key.D);
        private static string Arrows() => I(Key.ArrowUp) + I(Key.ArrowDown) + I(Key.ArrowLeft) + I(Key.ArrowRight);

        private static string KeyboardMovementHint() =>
            $"{Wasd()}:移動 / {I(Key.Shift)}   +移動:走る / " +
            $"{I(Key.Ctrl)}  +移動:向きのみ変える/ {I(Key.Alt)} +移動:斜め移動 / " +
            $"{I(Key.X)}:足踏み / {I(Key.Tab)} :メニュー";

        private static string KeyboardItemMenuHint() =>
            $"アイテムメニュー: {Arrows()}:選択 / " +
            $"{I(Key.Space)}:使う / {I(Key.E)}:入れ替え、拾う（足元） / {I(Key.Q)}:投げる / {I(Key.R)}:名付ける";
    }
}
