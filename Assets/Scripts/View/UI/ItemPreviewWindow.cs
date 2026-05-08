using TMPro;
using UnityEngine;

namespace View.UI
{
    public class ItemPreviewWindow : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private InventoryItemView _itemView;
        [SerializeField] private TMP_Text _itemInfo;

        public void SetVisibility(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        public void SetPreview(string title, ItemViewData itemData, string? note)
        {
            if (_titleText != null)
            {
                _titleText.text = title;
            }
            _itemView.Set(itemData);
            _itemInfo.text = $"{note ?? string.Empty}{itemData.info}";
        }
    }
}
