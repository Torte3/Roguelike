using TMPro;
using UnityEngine;

namespace View.UI
{
    public class GameOverWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _causeOfDeathText;
        public void SetData(int level, float score, string causeOfDeath)
        {
            _levelText.text = $"到達階層: {level}F";
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