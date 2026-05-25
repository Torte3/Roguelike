#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Service.Events;
using Game;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;
using View;

namespace Provider
{
    public class KeyCharacterPresenter
    {
        private readonly GameObject _lockPrefab;
        private readonly GameObject _keyPrefab;
        private readonly CompositeDisposable _disposables = new();
        public KeyCharacterPresenter(World world, SynchronizedCharacterView characters,
            SynchronizedIconEntityView iconEntities)
        {
            _lockPrefab = ObjectLoader.LoadPrefab("Lock");
            _keyPrefab = ObjectLoader.LoadPrefab("Key");
            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                _disposables.Clear();

                var map = mapChanged.Map;
                foreach (var lockedEntity in map.LockedEntities)
                {
                    SetLockIcon(characters, iconEntities, map, lockedEntity);
                }
            });
        }

        private void SetLockIcon(SynchronizedCharacterView characters, SynchronizedIconEntityView iconEntities, MapManager map, ILockedEntity lockedEntity)
        {
            if (!lockedEntity.KeyCharacters.Any())
                return;
            var lockedEntityView = iconEntities.Get(lockedEntity);
            var lockIcon = Object.Instantiate(_lockPrefab, lockedEntityView.transform)
                .GetComponent<StairsLock>();
            lockIcon.SetVisibility(lockedEntityView.GetComponent<SpriteRenderer>().enabled);
            lockIcon.SetCount(lockedEntity.KeyCharacters.Count);

            var lockIconDisposables = new CompositeDisposable();

            lockedEntity.Entity.OnDestroyed.Subscribe(_ =>
            {
                lockIcon.UnLock();
                lockIconDisposables.Dispose();
            }).AddTo(lockIconDisposables);

            foreach (var keyCharacterId in lockedEntity.KeyCharacters.ToList())
            {
                var keyCharacter = map.Characters.ById(keyCharacterId);
                var character = characters.Get(keyCharacter);
                var key = Object.Instantiate(_keyPrefab, character.transform);
                key.GetComponent<SpriteRenderer>().enabled = character.GetComponent<SpriteRenderer>().enabled;

                keyCharacter.Entity.OnDestroyed.Subscribe(_ => {
                    lockedEntity.KeyCharacters.Remove(keyCharacterId);
                    lockIcon.SetCount(lockedEntity.KeyCharacters.Count);
                    if (lockedEntity.KeyCharacters.Count == 0)
                    {
                        lockIcon.UnLock();
                        lockIconDisposables.Dispose();
                    }
                }).AddTo(lockIconDisposables);
            }

            lockIconDisposables.AddTo(_disposables);
        }
    }
}