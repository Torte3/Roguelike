using UnityEngine;

namespace View.UI
{
    public class TitleMenu : MonoBehaviour, IMenu
    {
        [SerializeField] private GameOverWindow _gameOverPanel;
        public bool CanClose => false;
        public void SetData(int level, float score, string causeOfDeath)
        {
            _gameOverPanel.SetData(level, score, causeOfDeath);
            _gameOverPanel.Show();
        }
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _gameOverPanel.Hide();
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }
    }
}