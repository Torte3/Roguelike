using System;
using Configuration;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using View.Playback;

namespace View
{
    public class BGMManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _normalBGM;
        [SerializeField] private AudioClip _stolenBGM;
        [SerializeField] private AudioClip _shopBGM;
        [SerializeField] private AudioClip _monsterHouseBGM;

        private void Awake()
        {
            Settings.GlobalSettings.BGMVolume.Value
                .SubscribeIncludingCurrentValue(volume => _audioSource.volume = volume / 100f)
                .AddTo(this);
        }

        internal void Play(BgmTrack track)
        {
            Log.Debug($"[BGM]Change BGM to {track}");
            ChangeBGM(track switch
            {
                BgmTrack.Normal => _normalBGM,
                BgmTrack.Stolen => _stolenBGM,
                BgmTrack.Shop => _shopBGM,
                BgmTrack.MonsterHouse => _monsterHouseBGM,
                _ => throw new ArgumentOutOfRangeException(nameof(track), track, null),
            });
        }

        private void ChangeBGM(AudioClip? clip)
        {
            if (clip == null)
            {
                return;
            }

            if (_audioSource.clip == clip)
            {
                return;
            }

            _audioSource.clip = clip;
            _audioSource.Play();
        }
    }
}