#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Utilities;
using VContainer;
using View.Effects;
using View.Entities;
using View.Playback.Figures;
using View.Playback.Values;
using Object = UnityEngine.Object;

namespace View.Playback.Drawing
{
    public sealed class EntityBoard
    {
        private readonly Dictionary<Guid, EntityFigure> _figures = new();
        private readonly EffectViewSpawner _effectViewSpawner;
        private readonly CameraFollowTarget _camera;

        [Inject]
        public EntityBoard(EffectViewSpawner effectViewSpawner, CameraFollowTarget camera)
        {
            _effectViewSpawner = effectViewSpawner;
            _camera = camera;
        }

        internal Vector2Int? PositionOf(Guid key)
        {
            return _figures.TryGetValue(key, out var figure) ? figure.Position : null;
        }

        internal void Clear()
        {
            foreach (var key in _figures.Keys.ToList())
                Remove(key);
        }

        internal void Add(Guid key, FigureBlueprint blueprint, Vector2Int position, Sprite? icon, bool isShiny, bool isVisible)
        {
            Add(key, position, icon, isShiny, isVisible, Instantiate(blueprint.PrefabName, blueprint.Create));
        }

        internal void AddCharacter(Guid key, Vector2Int position, bool isShiny, bool isVisible, CharacterAppearance appearance)
        {
            var figure = Add(key, position, null, isShiny, isVisible,
                Instantiate("Character", view => new CharacterFigure(view, _effectViewSpawner, appearance)));
            figure.SetCharge(appearance.Charge);
            figure.SetKeyHolder(appearance.IsKeyHolder);
            if (appearance.FollowsCamera)
                _camera.SetTarget(figure.View.gameObject);
        }

        internal void Remove(Guid key)
        {
            if (_figures.Remove(key, out var figure))
                figure.Destroy();
        }

        internal void Place(Guid key, Vector2Int position, bool isVisible)
        {
            WithFigure(key, figure =>
            {
                figure.Position = position;
                figure.Sprite.SetVisibility(isVisible);
                figure.View.SetPosition(position);
            });
        }

        internal void Walk(Guid key, Vector2Int position, bool isVisibleAfter, float seconds)
        {
            WithFigure(key, figure =>
            {
                Glide(figure, position, isVisibleAfter, seconds);
                figure.PlayWalkAnimation();
            });
        }

        internal void Glide(Guid key, Vector2Int position, bool isVisibleAfter, float seconds)
        {
            WithFigure(key, figure => Glide(figure, position, isVisibleAfter, seconds));
        }

        internal void SetVisibility(Guid key, bool isVisible)
        {
            WithFigure(key, figure => figure.Sprite.SetVisibility(isVisible));
        }

        internal void Turn(Guid key, Direction8 direction)
        {
            WithFigure(key, figure => figure.Turn(direction));
        }

        internal void SetHp(Guid key, int hp, int maxHp)
        {
            WithFigure(key, figure => figure.SetHp(hp, maxHp));
        }

        internal void SetParticles(Guid key, IReadOnlyList<ParticleType> particles)
        {
            WithFigure(key, figure => figure.SetParticles(particles));
        }

        internal void SetCharge(Guid key, ChargePreview? charge)
        {
            WithFigure(key, figure => figure.SetCharge(charge));
        }

        internal void SetAffiliation(Guid key, AffiliationMarker marker)
        {
            WithFigure(key, figure => figure.SetAffiliation(marker));
        }

        internal void SetIcon(Guid key, Sprite? icon)
        {
            WithFigure(key, figure => figure.SetIcon(icon));
        }

        internal void SetUsable(Guid key, bool isUsable)
        {
            WithFigure(key, figure => figure.SetUsable(isUsable));
        }

        internal void SetLockCount(Guid key, int lockCount)
        {
            WithFigure(key, figure => figure.SetLockCount(lockCount));
        }

        internal void SetKeyHolder(Guid key, bool isKeyHolder)
        {
            WithFigure(key, figure => figure.SetKeyHolder(isKeyHolder));
        }

        internal void SetInteractable(Guid key, bool isInteractable)
        {
            WithFigure(key, figure => figure.SetInteractable(isInteractable));
        }

        internal void PlayAttack(Guid key)
        {
            WithFigure(key, figure => figure.PlayAttack());
        }

        internal void Shake(Guid key)
        {
            WithFigure(key, figure => figure.View.transform.DOShakePosition(0.5f, 0.1f).SetLink(figure.View.gameObject));
        }

        private void Glide(EntityFigure figure, Vector2Int position, bool isVisibleAfter, float seconds)
        {
            figure.Position = position;
            figure.Sprite.SetVisibility(true);
            figure.View.MoveTo(position, seconds, () => figure.Sprite.SetVisibility(isVisibleAfter));
        }

        private void WithFigure(Guid key, Action<EntityFigure> apply)
        {
            if (_figures.TryGetValue(key, out var figure))
                apply(figure);
        }

        private EntityFigure Instantiate(string prefabName, Func<EntityView, EntityFigure> create)
        {
            return create(Object.Instantiate(ObjectLoader.LoadPrefab(prefabName)).GetComponent<EntityView>());
        }

        private EntityFigure Add(Guid key, Vector2Int position, Sprite? icon, bool isShiny, bool isVisible, EntityFigure figure)
        {
            figure.Position = position;
            _figures.Add(key, figure);
            figure.View.SetPosition(position);
            figure.SetIcon(icon);
            if (isShiny)
                figure.View.GetComponent<ParticleController>().Add(ParticleType.ShinyStar);
            figure.Sprite.SetVisibility(isVisible);
            return figure;
        }
    }
}
