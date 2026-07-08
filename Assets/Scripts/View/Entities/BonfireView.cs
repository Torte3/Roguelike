using UnityEngine;

namespace View.Entities
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