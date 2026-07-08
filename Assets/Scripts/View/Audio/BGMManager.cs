using System;
using Configuration;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using View.Playback.Values;

namespace View.Audio
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

        internal void Play(BGMTrack track)
        {
            Log.Debug($"[BGM]Change BGM to {track}");
            ChangeBGM(track switch
            {
                BGMTrack.Normal => _normalBGM,
                BGMTrack.Stolen => _stolenBGM,
                BGMTrack.Shop => _shopBGM,
                BGMTrack.MonsterHouse => _monsterHouseBGM,
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