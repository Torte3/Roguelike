#nullable enable
using TMPro;
using UnityEngine;

namespace View.UI
{
    public class ItemPreviewView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private ItemPreviewItemView _itemView;
        [SerializeField] private TMP_Text _itemInfo;

        public void SetVisibility(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        public void SetPreview(string title, ItemPreviewViewData data, string? note = null)
        {
            if (_titleText != null)
                _titleText.text = title;
            _itemView.Set(data);
            _itemInfo.text = $"{note ?? string.Empty}{data.info}";
        }
    }
}
