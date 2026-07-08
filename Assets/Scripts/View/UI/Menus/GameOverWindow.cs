using TMPro;
using UnityEngine;

namespace View.UI.Menus
{
    public class GameOverWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _causeOfDeathText;
        public void SetData(string levelText, float score, string causeOfDeath)
        {
            _levelText.text = levelText;
            _scoreText.text = $"スコア: {score:N0}";
            _causeOfDeathText.text = causeOfDeath;
        }
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}