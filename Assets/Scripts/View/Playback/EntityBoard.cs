#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using Utilities;
using VContainer;
using Object = UnityEngine.Object;

namespace View.Playback
{
    public sealed class EntityBoard
    {
        private sealed class Entry
        {
            public bool Walks;
            public Vector2Int Position;
            public EntityView View = null!;
            public SpriteView Sprite = null!;
            public CharacterView? Character;
            public SpriteRenderer? InteractionArrow;
            public StairsLock? Lock;
            public GameObject? Key;
            public IReadOnlyList<ParticleType> Particles = Array.Empty<ParticleType>();
            public ChargePreview? Charge;
            public readonly List<GameObject> ChargePreviews = new();
        }

        private readonly Dictionary<EntityKey, Entry> _entries = new();
        private readonly EffectViewSpawner _effectViewSpawner;
        private readonly ShownSight _sight;
        private readonly CameraFollowTarget _camera;

        [Inject]
        public EntityBoard(EffectViewSpawner effectViewSpawner, ShownSight sight, CameraFollowTarget camera)
        {
            _effectViewSpawner = effectViewSpawner;
            _sight = sight;
            _camera = camera;

            sight.OnChanged.Subscribe(changed =>
            {
                foreach (var entry in _entries.Values)
                {
                    if (entry.Charge != null && entry.Charge.Area.Any(changed.Contains))
                        DrawChargePreview(entry);
                }
            });
        }

        internal Vector2Int? PositionOf(EntityKey key)
        {
            return _entries.TryGetValue(key, out var entry) ? entry.Position : null;
        }

        internal void Clear()
        {
            foreach (var key in _entries.Keys.ToList())
                Remove(key);
        }

        internal void Add(EntityKey key, string prefabName, Vector2Int position, Sprite? icon, bool isShiny, bool isVisible)
        {
            Add(key, prefabName, position, icon, isShiny, isVisible, null);
        }

        internal void AddCharacter(EntityKey key, Vector2Int position, bool isShiny, bool isVisible, CharacterSpec spec)
        {
            var entry = Add(key, "Character", position, null, isShiny, isVisible, built => BuildCharacter(built, spec));
            if (!spec.IsPlayer)
                return;

            var arrow = Object.Instantiate(ObjectLoader.LoadPrefab("Arrow"), entry.View.transform);
            arrow.GetComponent<CharacterArrow>().SetCharacter(entry.Character!);
            _camera.SetTarget(entry.View.gameObject);
        }

        internal void Remove(EntityKey key)
        {
            if (!_entries.Remove(key, out var entry))
                return;
            entry.ChargePreviews.ForEach(Object.Destroy);
            Object.Destroy(entry.View.gameObject);
        }

        internal void Place(EntityKey key, Vector2Int position, bool isVisible)
        {
            WithEntry(key, entry =>
            {
                entry.Position = position;
                entry.Sprite.SetVisibility(isVisible);
                entry.View.SetPosition(position);
            });
        }

        internal void Walk(EntityKey key, Vector2Int position, bool isVisibleAfter, float seconds)
        {
            WithEntry(key, entry =>
            {
                Glide(entry, position, isVisibleAfter, seconds);
                if (entry.Walks)
                    entry.Character!.PlayWalkAnimation().Forget();
            });
        }

        internal void Glide(EntityKey key, Vector2Int position, bool isVisibleAfter, float seconds)
        {
            WithEntry(key, entry => Glide(entry, position, isVisibleAfter, seconds));
        }

        internal void SetVisibility(EntityKey key, bool isVisible)
        {
            WithEntry(key, entry => entry.Sprite.SetVisibility(isVisible));
        }

        internal void Turn(EntityKey key, Direction8 direction)
        {
            WithEntry(key, entry => entry.Character?.Turn(direction));
        }

        internal void SetHp(EntityKey key, int hp, int maxHp)
        {
            WithEntry(key, entry => entry.Character?.UpdateHpBar(maxHp, hp));
        }

        internal void SetParticles(EntityKey key, IReadOnlyList<ParticleType> particles)
        {
            WithEntry(key, entry => SetParticles(entry, particles));
        }

        internal void SetCharge(EntityKey key, ChargePreview? charge)
        {
            WithEntry(key, entry =>
            {
                entry.Charge = charge;
                DrawChargePreview(entry);
            });
        }

        internal void SetChargeTurns(EntityKey key, int turns)
        {
            WithEntry(key, entry =>
            {
                if (entry.Charge == null)
                    return;
                entry.Charge = entry.Charge with { Turns = turns };
                DrawChargePreview(entry);
            });
        }

        internal void SetAffiliation(EntityKey key, bool isEnemy, bool isAlly)
        {
            WithEntry(key, entry => entry.Character?.UpdateGroupMarker(isEnemy, isAlly));
        }

        internal void SetIcon(EntityKey key, Sprite? icon)
        {
            WithEntry(key, entry => SetIcon(entry, icon));
        }

        internal void SetUsable(EntityKey key, bool isUsable)
        {
            WithEntry(key, entry =>
            {
                if (entry.View.TryGetComponent<BonfireView>(out var bonfire))
                    bonfire.ShowFire(isUsable);
            });
        }

        internal void SetLockCount(EntityKey key, int lockCount)
        {
            WithEntry(key, entry => SetLockCount(entry, lockCount));
        }

        internal void SetKeyHolder(EntityKey key, bool isKeyHolder)
        {
            WithEntry(key, entry => SetKeyHolder(entry, isKeyHolder));
        }

        internal void SetInteractable(EntityKey key, bool isInteractable)
        {
            WithEntry(key, entry => SetInteractable(entry, isInteractable));
        }

        internal void PlayAttack(EntityKey key)
        {
            WithEntry(key, entry => entry.Character?.PlayAttackAnimation());
        }

        internal void Shake(EntityKey key)
        {
            WithEntry(key, entry => entry.View.transform.DOShakePosition(0.5f, 0.1f).SetLink(entry.View.gameObject));
        }

        private static void Glide(Entry entry, Vector2Int position, bool isVisibleAfter, float seconds)
        {
            entry.Position = position;
            entry.Sprite.SetVisibility(true);
            entry.View.MoveTo(position, seconds, () => entry.Sprite.SetVisibility(isVisibleAfter));
        }

        private void WithEntry(EntityKey key, Action<Entry> apply)
        {
            if (_entries.TryGetValue(key, out var entry))
                apply(entry);
        }

        private Entry Add(EntityKey key, string prefabName, Vector2Int position, Sprite? icon, bool isShiny,
            bool isVisible, Action<Entry>? build)
        {
            var view = Object.Instantiate(ObjectLoader.LoadPrefab(prefabName)).GetComponent<EntityView>();
            var entry = new Entry
            {
                Position = position,
                View = view,
                Sprite = view.GetComponent<SpriteView>(),
                Character = view.GetComponent<CharacterView>(),
            };
            _entries.Add(key, entry);
            view.SetPosition(position);
            SetIcon(entry, icon);
            if (isShiny)
                view.GetComponent<ParticleController>().Add(ParticleType.ShinyStar);
            build?.Invoke(entry);
            entry.Sprite.SetVisibility(isVisible);
            return entry;
        }

        private void BuildCharacter(Entry entry, CharacterSpec spec)
        {
            var view = entry.Character!;
            entry.Walks = !spec.IsFlying;
            view.Construct(spec.TypeName);
            view.UpdateGroupMarker(spec.IsEnemy, spec.IsAlly);
            entry.View.GetComponent<OverrideSprite>().SetTexture(
                spec.TypeName, spec.SubtypeName, spec.TypeName == "Human");
            if (spec.IsBoss)
                view.SetScale(1.5f);
            view.Turn(spec.Direction);
            view.UpdateHpBar(spec.MaxHp, spec.Hp);
            SetParticles(entry, spec.Particles);
            entry.Charge = spec.Charge;
            DrawChargePreview(entry);
            SetKeyHolder(entry, spec.IsKeyHolder);
        }

        private static void SetIcon(Entry entry, Sprite? icon)
        {
            if (icon != null)
                entry.View.GetComponent<SpriteRenderer>().sprite = icon;
        }

        private static void SetInteractable(Entry entry, bool isInteractable)
        {
            if (entry.InteractionArrow == null)
            {
                if (!isInteractable)
                    return;
                entry.InteractionArrow = Object.Instantiate(ObjectLoader.LoadPrefab("EvArrow"), entry.View.transform)
                    .GetComponent<SpriteRenderer>();
                entry.Sprite.UpdateVisibility();
            }

            entry.InteractionArrow.color = isInteractable ? Color.green : Color.clear;
        }

        private static void SetParticles(Entry entry, IReadOnlyList<ParticleType> particles)
        {
            var controller = entry.View.GetComponent<ParticleController>();
            foreach (var particle in particles.Except(entry.Particles))
                controller.Add(particle);
            foreach (var particle in entry.Particles.Except(particles))
                controller.Remove(particle);
            entry.Particles = particles;
            entry.Sprite.UpdateVisibility();
        }

        private static void SetLockCount(Entry entry, int lockCount)
        {
            if (lockCount > 0)
            {
                if (entry.Lock == null)
                {
                    entry.Lock = Object.Instantiate(ObjectLoader.LoadPrefab("Lock"), entry.View.transform)
                        .GetComponent<StairsLock>();
                    entry.Sprite.UpdateVisibility();
                }
                entry.Lock.SetCount(lockCount);
            }
            else if (entry.Lock != null)
            {
                entry.Lock.UnLock();
                entry.Lock = null;
            }
        }

        private static void SetKeyHolder(Entry entry, bool isKeyHolder)
        {
            if (isKeyHolder && entry.Key == null)
            {
                entry.Key = Object.Instantiate(ObjectLoader.LoadPrefab("Key"), entry.View.transform);
                entry.Sprite.UpdateVisibility();
            }
            else if (!isKeyHolder && entry.Key != null)
            {
                Object.Destroy(entry.Key);
                entry.Key = null;
            }
        }

        private void DrawChargePreview(Entry entry)
        {
            entry.ChargePreviews.ForEach(Object.Destroy);
            entry.ChargePreviews.Clear();
            if (entry.Charge is not { } charge)
                return;

            var color = charge.Color;
            color.a = 0.25f;
            entry.ChargePreviews.AddRange(_effectViewSpawner.SpawnChargePreview(
                charge.Area.Where(_sight.Contains), color, charge.Turns, entry.Position));
        }
    }
}
