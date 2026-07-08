#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Service.Characters.Behavior;
using Game;
using R3;
using UnityEngine;
using VContainer;
using View;
using View.Playback;
using View.UI;

namespace Provider
{
    public class EffectPreviewPresenter
    {
        private readonly IReadOnlyWorld _world;
        private readonly PlaybackQueue _playback;
        private readonly EffectViewSpawner _effectViewSpawner;
        private readonly InventoryView _inventoryView;
        private readonly CharacterControlInputReceiver _actionReceiver;
        private List<GameObject> _previews = new();
        private int _refreshRequests;

        [Inject]
        public EffectPreviewPresenter(IReadOnlyWorld world, PlaybackQueue playback, EffectViewSpawner effectViewSpawner,
            InventoryView inventoryView, CharacterControlInputReceiver actionReceiver)
        {
            _world = world;
            _playback = playback;
            _effectViewSpawner = effectViewSpawner;
            _inventoryView = inventoryView;
            _actionReceiver = actionReceiver;

            actionReceiver.IsWaitingForAction.Subscribe(_ => RefreshWhenPlaybackIsIdle().Forget());
            Observable.Merge(inventoryView.Focus.AsUnitObservable(), world.Events.OnRecorded.AsUnitObservable())
                .Where(_ => actionReceiver.IsWaitingForAction.CurrentValue)
                .Subscribe(_ => RefreshWhenPlaybackIsIdle().Forget());
        }

        private async UniTaskVoid RefreshWhenPlaybackIsIdle()
        {
            var request = ++_refreshRequests;
            if (!_actionReceiver.IsWaitingForAction.CurrentValue)
            {
                ClearPreviews();
                return;
            }

            await _playback.WaitUntilIdle();
            if (request != _refreshRequests || _world.CurrentMap is not { } map)
                return;

            ClearPreviews();
            var focus = _inventoryView.Focus.CurrentValue;
            if (focus.IsOnEmpty)
                return;

            if (map.PlayerCharacter.PreviewEffectArea(focus.ToItemFocus()) is not { } effectArea)
                return;

            var color = effectArea.Color;
            color.a = 0.25f;
            _previews = _effectViewSpawner.SpawnPreview(effectArea.Positions, color);
        }

        private void ClearPreviews()
        {
            _previews.ForEach(preview => Object.Destroy(preview));
            _previews.Clear();
        }
    }
}
