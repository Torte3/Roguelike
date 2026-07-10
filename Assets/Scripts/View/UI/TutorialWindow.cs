using UnityEngine;

namespace View.UI
{
    // チュートリアルウィンドウ。ボタンは持たず、表示/非表示と中身の切り替えのみ。
    // 中身は種類ごとに名前付きフィールドで持つ（index は TutorialType の値と一致）。
    // メニュースタックに載せて表示するため IMenu を実装する。
    public class TutorialWindow : MonoBehaviour, IMenu
    {
        [SerializeField] private GameObject _firstGameContent;
        [SerializeField] private GameObject _shopContent;
        [SerializeField] private GameObject _magicCircleContent;
        [SerializeField] private GameObject _floor30Content;

        public bool CanClose => false;

        // index（TutorialType の値: 0=FirstGame,1=Shop,2=MagicCircle,3=Floor30）に対応する中身だけを表示する。
        public void SetContent(int index)
        {
            SetActiveSafe(_firstGameContent, index == 0);
            SetActiveSafe(_shopContent, index == 1);
            SetActiveSafe(_magicCircleContent, index == 2);
            SetActiveSafe(_floor30Content, index == 3);
        }

        private static void SetActiveSafe(GameObject content, bool active)
        {
            if (content != null)
                content.SetActive(active);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }
    }
}
