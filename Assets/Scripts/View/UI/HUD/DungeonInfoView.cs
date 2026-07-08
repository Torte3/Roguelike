using TMPro;
using UnityEngine;

namespace View.UI.HUD
{
    public class DungeonInfoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _floorText;

        public void SetInfo(string text)
        {
            _floorText.text = text;
        }
    }
}