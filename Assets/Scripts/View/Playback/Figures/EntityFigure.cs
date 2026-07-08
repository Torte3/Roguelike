#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;
using View.Effects;
using View.Entities;
using View.Playback.Values;
using Object = UnityEngine.Object;

namespace View.Playback.Figures
{
    internal class EntityFigure
    {
        private IReadOnlyList<ParticleType> _particles = Array.Empty<ParticleType>();

        public EntityFigure(EntityView view)
        {
            View = view;
            Sprite = view.GetComponent<SpriteView>();
        }

        public Vector2Int Position { get; set; }
        public EntityView View { get; }
        public SpriteView Sprite { get; }

        public virtual void Destroy()
        {
            Object.Destroy(View.gameObject);
        }

        public virtual void PlayWalkAnimation()
        {
        }

        public virtual void Turn(Direction8 direction)
        {
        }

        public virtual void SetHp(int hp, int maxHp)
        {
        }

        public virtual void SetAffiliation(AffiliationMarker marker)
        {
        }

        public virtual void SetCharge(ChargePreview? charge)
        {
        }

        public virtual void PlayAttack()
        {
        }

        public virtual void SetUsable(bool isUsable)
        {
        }

        public virtual void SetLockCount(int lockCount)
        {
        }

        public virtual void SetKeyHolder(bool isKeyHolder)
        {
        }

        public virtual void SetInteractable(bool isInteractable)
        {
        }

        public void SetIcon(Sprite? icon)
        {
            if (icon != null)
                View.GetComponent<SpriteRenderer>().sprite = icon;
        }

        public void SetParticles(IReadOnlyList<ParticleType> particles)
        {
            var controller = View.GetComponent<ParticleController>();
            foreach (var particle in particles.Except(_particles))
                controller.Add(particle);
            foreach (var particle in _particles.Except(particles))
                controller.Remove(particle);
            _particles = particles;
            Sprite.UpdateVisibility();
        }
    }
}
