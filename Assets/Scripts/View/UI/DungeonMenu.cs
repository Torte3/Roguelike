using Unity.Logging;
using UnityEngine;

namespace View.UI
{
    public class DungeonMenu : MonoBehaviour, IMenu
    {
        [SerializeField] private InventoryView _inventory;
        public bool CanClose => false;

        public void Show()
        {
            Log.Debug($"[Menu]DungeonMenu Show");
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            Log.Debug($"[Menu]DungeonMenu Hide");
            gameObject.SetActive(false);
        }

        public void Enable()
        {
            Log.Debug($"[Menu]DungeonMenu Enable");
            _inventory.EnableAllItems();
        }

        public void Disable()
        {
            Log.Debug($"[Menu]DungeonMenu Disable");
            _inventory.DisableAllItems();
        }
    }
}