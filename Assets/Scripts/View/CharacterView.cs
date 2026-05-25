using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using Utilities;

namespace View
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

        public void Construct(string characterTypeName, bool isEnemy, bool isAlly)
        {
            var animation = ObjectLoader.LoadAnimation(characterTypeName);

            _animator = GetComponent<Animator>();
            _animator.runtimeAnimatorController = animation;

            UpdateGroupMarker(isEnemy, isAlly);
        }

        public void SetScale(float value)
        {
            transform.localScale = new Vector3(value, value, 1);
        }

        public void Turn(Direction8 direction)
        {
            _direction.OnNext(direction);
        }

        public void UpdateGroupMarker(bool isEnemy, bool isAlly)
        {
            var groupMarkerColor = (isEnemy, isAlly) switch
            {
                (true, false) => new Color(1, 0, 0, 0.5f),
                (false, true) => new Color(0, 1, 0, 0.5f),
                (false, false) => Color.clear,
                (true, true) => throw new InvalidOperationException("A character cannot be both an enemy and an ally."),
            };
            var minimapMarkerColor = (isEnemy, isAlly) switch
            {
                (true, false) => new Color(1, 0, 0, 1f),
                (false, true) => new Color(0, 1, 0, 1f),
                (false, false) => new Color(1, 1, 1, 1f),
                (true, true) => throw new InvalidOperationException("A character cannot be both an enemy and an ally."),
            };
            _groupMarker.color = groupMarkerColor;
            _minimapMarker.color = minimapMarkerColor;
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