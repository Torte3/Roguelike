#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Configuration;
using UnityEngine;
using Utilities;
using VContainer;
using Object = UnityEngine.Object;

namespace View.Playback
{
    public sealed class ProjectileLayer
    {
        private readonly ShownSight _sight;

        [Inject]
        public ProjectileLayer(ShownSight sight)
        {
            _sight = sight;
        }

        internal float Fly(Sprite icon, Vector2Int from, Vector2Int to)
        {
            var path = VisiblePath(from, to);
            if (path.Count < 2)
                return 0;

            var start = path.First();
            var end = path.Last();
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

        private List<Vector2Int> VisiblePath(Vector2Int from, Vector2Int to)
        {
            var cells = Mathf.RoundToInt(VectorExtension.ChebyshevDistance(from, to));
            var step = new Vector2Int(Math.Sign(to.x - from.x), Math.Sign(to.y - from.y));
            return Enumerable.Range(0, cells + 1)
                .Select(index => from + step * index)
                .Where(_sight.Contains)
                .ToList();
        }
    }
}
