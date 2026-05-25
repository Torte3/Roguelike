using UnityEngine;

namespace View
{
    public class BonfireView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _fire;
        public void ShowFire(bool isFire)
        {
            _fire.enabled = isFire;
        }
    }
}