#nullable enable
using TMPro;
using UnityEngine;

namespace View.UI
{
    public class InfoMenu : MonoBehaviour, IMenu
    {
        public bool CanClose => false;
        [SerializeField] private TextMeshProUGUI _infoTitleText;
        [SerializeField] private TextMeshProUGUI _infoText;

        public void SetInfo(string infoTitle, string info)
        {
            _infoTitleText.text = infoTitle;
            _infoText.text = info;
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