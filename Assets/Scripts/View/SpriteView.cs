using UnityEngine;

namespace View
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteView : MonoBehaviour
    {
        private bool _isVisible;

        public Vector3 Position()
        {
            return transform.position;
        }

        public bool GetVisibility()
        {
            return _isVisible;
        }

        public void SetVisibility(bool visible)
        {
            _isVisible = visible;
            UpdateVisibility();
        }

        public void UpdateVisibility()
        {
            GetComponent<SpriteRenderer>().enabled = _isVisible;
            foreach (var child in transform.GetComponentsInChildren<SpriteRenderer>())
            {
                child.enabled = _isVisible;
            }

            foreach (var child in transform.GetComponentsInChildren<MeshRenderer>())
            {
                child.enabled = _isVisible;
            }

            foreach (var child in transform.GetComponentsInChildren<ParticleSystemRenderer>())
            {
                if (child.sharedMaterial == null)
                    continue;
                child.enabled = _isVisible;
            }
        }
    }
}