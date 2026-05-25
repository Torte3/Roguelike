using UnityEngine;
using Utilities;

namespace View
{
    public static class WorldIconPopup
    {
        private const float DefaultScale = 0.8f;
        private const float DefaultMoveY = 0.8f;
        private const int DefaultDisplayMilliseconds = 1000;

        public static void Show(Sprite icon, Vector2 worldPosition, int displayMilliseconds = DefaultDisplayMilliseconds)
        {
            if (icon == null)
                return;

            var popupObject = Object.Instantiate(ObjectLoader.LoadPrefab("PopUp"));
            popupObject.transform.position = worldPosition;
            popupObject.transform.localScale = Vector3.one * DefaultScale;

            var lifeTimer = popupObject.GetComponent<LifeTimer>();
            if (lifeTimer != null)
            {
                lifeTimer.LifeTimeMilliseconds = displayMilliseconds;
            }

            var spriteRenderer = popupObject.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Object.Destroy(popupObject);
                return;
            }
            spriteRenderer.sprite = icon;

            var motion = popupObject.GetComponent<PopupIconMotion>();
            if (motion == null)
            {
                motion = popupObject.AddComponent<PopupIconMotion>();
            }
            motion.MoveY = DefaultMoveY;
            motion.DurationSeconds = Mathf.Max(0.1f, displayMilliseconds / 1000f);
        }
    }
}
