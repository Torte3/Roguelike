#nullable enable
using Configuration;
using UnityEngine;
using Utilities;
using View.Entities;
using Object = UnityEngine.Object;

namespace View.Playback.Drawing
{
    public sealed class ProjectileLayer
    {
        internal float Fly(Sprite icon, Vector2Int start, Vector2Int end)
        {
            var duration = SecondsBetween(start, end);
            var view = Object.Instantiate(ObjectLoader.LoadPrefab("Entity")).GetComponent<EntityView>();
            view.GetComponent<SpriteRenderer>().sprite = icon;
            view.GetComponent<SpriteView>().SetVisibility(true);
            view.SetPosition(start);
            view.MoveTo(end, duration);
            Object.Destroy(view.gameObject, duration);
            return duration;
        }

        internal float SecondsBetween(Vector2Int start, Vector2Int end)
        {
            return VectorExtension.ChebyshevDistance(start, end) *
                Settings.GlobalSettings.ThrowMilliseconds.CurrentValue / 1000f;
        }
    }
}
