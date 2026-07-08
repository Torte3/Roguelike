#nullable enable
using UnityEngine;
using Utilities;
using View.Entities;

namespace View.Playback.Figures
{
    internal class InteractiveFigure : EntityFigure
    {
        private SpriteRenderer? _interactionArrow;

        public InteractiveFigure(EntityView view) : base(view)
        {
        }

        public override void SetInteractable(bool isInteractable)
        {
            if (_interactionArrow == null)
            {
                if (!isInteractable)
                    return;
                _interactionArrow = Object.Instantiate(ObjectLoader.LoadPrefab("EvArrow"), View.transform)
                    .GetComponent<SpriteRenderer>();
                Sprite.UpdateVisibility();
            }

            _interactionArrow.color = isInteractable ? Color.green : Color.clear;
        }
    }
}
