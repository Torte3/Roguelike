using Unity.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View.UI
{
    [RequireComponent(typeof(Selectable))]
    internal class AutoSelecter : MonoBehaviour
    {
        private Selectable _selectable;
        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }
        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == null && _selectable.interactable)
            {
                Log.Info($"[Menu]AutoSelect: {gameObject}");
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
    }
}