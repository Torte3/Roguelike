using TMPro;
using UnityEngine;

namespace View.UI
{
    public class StatView : MonoBehaviour
    {
        [SerializeField] private StatLine _hp;
        [SerializeField] private TMP_Text _money;
        [SerializeField] private TMP_Text _inventory;
        public void SetHp(float maxValue, float value)
        {
            _hp.SetValue(maxValue, value);
        }
        public void SetMoney(int money)
        {
            _money.text = $"{money}G";
        }
        public void SetInventory(int currentItems, int maxCapacity)
        {
            _inventory.text = $"{currentItems}/{maxCapacity}";
        }
        public void SetTextColor(Color color)
        {
            _hp.SetTextColor(color);
        }
    }
}