#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Setting;
using Domain.Service.Events;
using Game;
using Provider.Input;
using R3;
using UnityEngine;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SynchronizedCharacterView : SynchronizedEntityView<ICharacter, CharacterView>
    {
        private readonly SerialDisposable _disposable = new();
        protected override InputReceiver _inputReceiver { get; init; }
        private readonly EffectViewSpawner _effectViewSpawner;
        protected override GameManager _gameManager { get; init; }
        protected override World _world { get; init; }

        protected override EntityView GetEntityView(CharacterView view)
        {
            return view.GetComponent<EntityView>();
        }

        [Inject]
        public SynchronizedCharacterView(InputReceiver receiver, EffectViewSpawner effectViewSpawner, GameManager gameManager, World world)
        {
            _inputReceiver = receiver;
            _effectViewSpawner = effectViewSpawner;
            _gameManager = gameManager;
            _world = world;

            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                mapChanged.PreviousMap?.Characters.ForEach(character => Remove(character));
                _disposable.Disposable = mapChanged.Map.Characters.SubscribeIncludingCurrentItems(Add, Remove);
            });
        }

        protected override CharacterView ViewPrefab(ICharacter _)
        {
            return ObjectLoader.LoadPrefab("Character").GetComponent<CharacterView>();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        protected override void InitializeView(ICharacter character, CharacterView characterView)
        {
            var disposables = new CompositeDisposable();
            var player = _world.CurrentMap.Player;
            characterView.Construct(character.CharacterType.TypeName(), character.IsEnemy(player.Character),
                character.IsAlly(player.Character));
            if (character.IsBoss)
                characterView.SetScale(1.5f);

            character.Status.HpValue.SubscribeIncludingCurrentValue(hp =>
                characterView.UpdateHpBar(character.Status.MaxHp.CurrentValue, hp)).AddTo(characterView);
            character.Status.MaxHp.SubscribeIncludingCurrentValue(maxHp =>
                    characterView.UpdateHpBar(maxHp, character.CurrentHp))
                .AddTo(characterView);

            characterView.GetComponent<OverrideSprite>().SetTexture(
                character.CharacterType.TypeName(),
                character.CharacterType.SubtypeName(),
                character.CharacterType.TypeName() == "Human");

            character.Direction.Subscribe(direction => characterView.Turn(direction)).AddTo(characterView);

            character.Entity.OnMove
                .Where(move => !move.isThrown)
                .Where(move => !character.IsFlying)
                .Subscribe(move => characterView.PlayWalkAnimation().Forget())
                .AddTo(characterView);
            character.OnAttacked
                .Subscribe(useSkill => characterView.PlayAttackAnimation())
                .AddTo(characterView);

            var previews = new List<GameObject>();
            character.OnChargeActionUpdated.Subscribe(chargeAction =>
            {
                previews.ForEach(preview => GameObject.Destroy(preview));
                if (chargeAction.Turn > 0 && chargeAction.Data != null)
                {
                    var area = chargeAction.Data.Area;
                    var color = chargeAction.Data.Color;
                    color.a = 0.25f;
                    previews = _effectViewSpawner.SpawnChargePreview(
                        area, color, chargeAction.Turn, character.Entity.CurrentPosition);
                }
            }).AddTo(characterView);
            character.Entity.OnDestroyed.Subscribe(_ =>
            {
                previews.ForEach(preview => GameObject.Destroy(preview));
            }).AddTo(characterView);

            var particleController = characterView.GetComponent<ParticleController>();
            if (character.IsShiny)
                particleController.Add(ParticleType.ShinyStar);
            character.Status.Conditions.SubscribeIncludingCurrentItems(
                conditionAdded => particleController.Add(conditionAdded.ParticleType),
                conditionRemoved => particleController.Remove(conditionRemoved.ParticleType)
            ).AddTo(particleController);
        }

        protected override void CleanupView(ICharacter character, CharacterView characterView)
        {
        }
    }
}