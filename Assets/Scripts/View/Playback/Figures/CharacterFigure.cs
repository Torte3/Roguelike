#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utilities;
using View.Effects;
using View.Entities;
using View.Playback.Values;

namespace View.Playback.Figures
{
    internal sealed class CharacterFigure : InteractiveFigure
    {
        private readonly CharacterView _character;
        private readonly EffectViewSpawner _effectViewSpawner;
        private readonly bool _walks;
        private readonly List<GameObject> _chargePreviews = new();
        private GameObject? _key;

        public CharacterFigure(EntityView view, EffectViewSpawner effectViewSpawner, CharacterAppearance appearance) : base(view)
        {
            _character = view.GetComponent<CharacterView>();
            _effectViewSpawner = effectViewSpawner;
            _walks = appearance.Walks;
            _character.Construct(appearance.TypeName);
            _character.UpdateGroupMarker(appearance.Marker);
            view.GetComponent<OverrideSprite>().SetTexture(appearance.TypeName, appearance.SubtypeName, appearance.UsesDirectionalSprites);
            if (appearance.Scale.HasValue)
                _character.SetScale(appearance.Scale.Value);
            _character.Turn(appearance.Direction);
            _character.UpdateHpBar(appearance.MaxHp, appearance.Hp);
            SetParticles(appearance.Particles);
            if (appearance.HasDirectionArrow)
                Object.Instantiate(ObjectLoader.LoadPrefab("Arrow"), view.transform).GetComponent<CharacterArrow>().SetCharacter(_character);
        }

        public override void Destroy()
        {
            _chargePreviews.ForEach(Object.Destroy);
            base.Destroy();
        }

        public override void PlayWalkAnimation()
        {
            if (_walks)
                _character.PlayWalkAnimation().Forget();
        }

        public override void Turn(Direction8 direction)
        {
            _character.Turn(direction);
        }

        public override void SetHp(int hp, int maxHp)
        {
            _character.UpdateHpBar(maxHp, hp);
        }

        public override void SetAffiliation(AffiliationMarker marker)
        {
            _character.UpdateGroupMarker(marker);
        }

        public override void PlayAttack()
        {
            _character.PlayAttackAnimation();
        }

        public override void SetCharge(ChargePreview? charge)
        {
            _chargePreviews.ForEach(Object.Destroy);
            _chargePreviews.Clear();
            if (charge == null)
                return;

            _chargePreviews.AddRange(_effectViewSpawner.SpawnChargePreview(charge.Area, charge.Color, charge.Turns, Position));
        }

        public override void SetKeyHolder(bool isKeyHolder)
        {
            if (isKeyHolder && _key == null)
            {
                _key = Object.Instantiate(ObjectLoader.LoadPrefab("Key"), View.transform);
                Sprite.UpdateVisibility();
            }
            else if (!isKeyHolder && _key != null)
            {
                Object.Destroy(_key);
                _key = null;
            }
        }
    }
}
