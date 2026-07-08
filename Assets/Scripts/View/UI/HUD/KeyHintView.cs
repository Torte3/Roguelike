#nullable enable
using TMPro;
using UnityEngine;

namespace View.UI.HUD
{
    public class KeyHintView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _movementHint = null!;
        [SerializeField] private TMP_Text _itemMenuHint = null!;

        private static class Pad
        {
            public const int Dpad = 398;
            public const int ButtonSouth = 13;
            public const int ButtonEast = 14;
            public const int ButtonWest = 15;
            public const int ButtonNorth = 16;
            public const int RightShoulder = 586;
            public const int RightTrigger = 584;
            public const int Start = 652;
            public const int Select = 651;
            public const int LeftShoulder = 585;
            public const int LeftStick = 460;
        }

        private static class Key
        {
            public const int W = 85;
            public const int A = 117;
            public const int S = 118;
            public const int D = 119;
            public const int ArrowUp = 162;
            public const int ArrowDown = 164;
            public const int ArrowLeft = 165;
            public const int ArrowRight = 163;
            public const int Shift = 239;
            public const int Ctrl = 212;
            public const int Alt = 183;
            public const int X = 152;
            public const int Tab = 184;
            public const int Space = 149;
            public const int E = 86;
            public const int Q = 84;
            public const int R = 87;
        }

        private bool? _isKeyboard;

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
