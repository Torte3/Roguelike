using Unity.Logging;
using UnityEngine;

namespace View
{
    public class BGMManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _normalBGM;
        [SerializeField] private AudioClip _stolenBGM;
        [SerializeField] private AudioClip _shopBGM;
        [SerializeField] private AudioClip _monsterHouseBGM;

        public void SetVolume(float volume)
        {
            _audioSource.volume = volume;
        }

        public void NormalBGM()
        {
            Log.Debug("[BGM]Change BGM to Normal");
            ChangeBGM(_normalBGM);
        }

        public void StolenBGM()
        {
            Log.Debug("[BGM]Change BGM to Stolen");
            ChangeBGM(_stolenBGM);
        }

        public void ShopBGM()
        {
            Log.Debug("[BGM]Change BGM to Shop");
            ChangeBGM(_shopBGM);
        }

        public void MonsterHouseBGM()
        {
            Log.Debug("[BGM]Change BGM to MonsterHouse");
            ChangeBGM(_monsterHouseBGM);
        }

        public void StopBGM()
        {
            _audioSource.Stop();
        }

        public void ChangeBGM(AudioClip? clip)
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