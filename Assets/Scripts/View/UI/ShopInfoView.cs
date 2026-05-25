using TMPro;
using UnityEngine;

namespace View.UI
{
    public class ShopInfoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _purchaseText;
        [SerializeField] private TMP_Text _saleText;

        public void SetVisibility(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        public void SetInfo(int purchase, int sell)
        {
            _purchaseText.text = $"購入金額: {purchase}G";
            _saleText.text = $"売却金額: {sell}G";
        }
    }
}