using UnityEngine;

namespace View.UI.Menus
{
    public class TitleMenu : MonoBehaviour, IMenu
    {
        [SerializeField] private GameOverWindow _gameOverPanel;
        public bool CanClose => false;
        public void SetData(string levelText, float score, string causeOfDeath)
        {
            _gameOverPanel.SetData(levelText, score, causeOfDeath);
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