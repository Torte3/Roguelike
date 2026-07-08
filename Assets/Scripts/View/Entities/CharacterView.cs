using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using Utilities;
using View.Effects;
using View.Playback.Values;

namespace View.Entities
{

    [RequireComponent(typeof(EntityView), typeof(Animator), typeof(ParticleController))]
    public class CharacterView : MonoBehaviour, IDirectional
    {
        private readonly ReactiveProperty<Direction8> _direction = new();
        private Animator _animator;
        [SerializeField] private SpriteRenderer _groupMarker;
        [SerializeField] private SpriteRenderer _minimapMarker;
        [SerializeField] private SpriteHpBar _hpBar;
        public ReadOnlyReactiveProperty<Direction8> Direction => _direction;

        public Direction8 GetDirection()
        {
            return Direction.CurrentValue;
        }

        public void Construct(string characterTypeName)
        {
            var animation = ObjectLoader.LoadAnimation(characterTypeName);

            _animator = GetComponent<Animator>();
            _animator.runtimeAnimatorController = animation;
        }

        public void SetScale(float value)
        {
            transform.localScale = new Vector3(value, value, 1);
        }

        public void Turn(Direction8 direction)
        {
            _direction.OnNext(direction);
        }

        internal void UpdateGroupMarker(AffiliationMarker marker)
        {
            _groupMarker.color = marker.GroupColor;
            _minimapMarker.color = marker.MinimapColor;
        }

        public void UpdateHpBar(float maxHp, float hp)
        {
            _hpBar.SetValue(maxHp, hp);
        }

        public void PlayAttackAnimation()
        {
            _animator.SetBool("Attack", true);
        }

        public async UniTask PlayWalkAnimation()
        {
            _animator.SetBool("Walk", true);
            _animator.speed = 2;
            var entityView = GetComponent<EntityView>();
            await UniTask.WaitUntil(() => !entityView.IsMoving, cancellationToken: destroyCancellationToken);
            _animator.SetBool("Walk", false);
            _animator.speed = 1;
        }
    }
}