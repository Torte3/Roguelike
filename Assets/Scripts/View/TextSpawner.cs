using TMPro;
using UnityEngine;
using Utilities;

namespace View
{
    public class TextSpawner : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;

        private TextMeshProUGUI _numberText;

        private void Reset()
        {
            canvas = GetComponent<Canvas>();
        }

        private void Awake()
        {
            canvas ??= GetComponent<Canvas>();
            _numberText = ObjectLoader.LoadPrefab("NumberText").GetComponent<TextMeshProUGUI>();
        }

        public TextMeshProUGUI SpawnNumber(Vector2 position, string text)
        {
            var instance = Instantiate(_numberText, canvas.transform);
            instance.text = text;
            instance.transform.position = position;
            return instance;
        }

        public void DeleteAll<T>() where T : Component
        {
            foreach (var component in canvas.GetComponentsInChildren<T>())
            {
                Destroy(component.gameObject);
            }
        }
    }
}
