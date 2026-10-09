using TMPro;
using UnityEngine;

namespace View
{
    public class StairsLock : MonoBehaviour
    {
        [SerializeField] private TMP_Text countText;

        private void Awake()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            var meshRenderer = countText.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        }

        public void SetCount(int count)
        {
            countText.text = count.ToString();
        }

        public void UnLock()
        {
            Destroy(gameObject);
        }
    }
}