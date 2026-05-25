using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace View
{
    [RequireComponent(typeof(SpriteView))]
    public class ParticleController : MonoBehaviour
    {
        private Dictionary<ParticleType, int> _particleCounter = new();
        private Dictionary<ParticleType, GameObject> _particles = new();
        [HideInInspector][SerializeField] private int _sortingLayerID;
        private SpriteView _spriteView;

        private void Awake()
        {
            _spriteView = GetComponent<SpriteView>();
        }

        public void Add(ParticleType particleType)
        {
            if (particleType == ParticleType.None)
                return;

            if (_particles.ContainsKey(particleType))
            {
                _particleCounter[particleType]++;
            }
            else
            {
                var EffectPrefab = ObjectLoader.LoadParticle(particleType.GetFileName());
                var particle = Instantiate(EffectPrefab, transform);
                foreach (var particleSystem in particle.GetComponentsInChildren<ParticleSystem>())
                {
                    particleSystem.gameObject.layer = gameObject.layer;
                    particleSystem.GetComponent<Renderer>().sortingLayerID = _sortingLayerID;
                }

                _particles.Add(particleType, particle);
                _particleCounter.Add(particleType, 1);
            }
            _spriteView.UpdateVisibility();
        }

        public void Remove(ParticleType particleType)
        {
            if (particleType == ParticleType.None)
                return;

            if (_particleCounter[particleType] > 1)
            {
                _particleCounter[particleType]--;
            }
            else
            {
                Destroy(_particles[particleType]);
                _particles.Remove(particleType);
                _particleCounter.Remove(particleType);
            }
            _spriteView.UpdateVisibility();
        }

        public void Clear()
        {
            foreach (var particle in _particles)
            {
                Destroy(particle.Value);
            }

            _particles.Clear();
            _particleCounter.Clear();
        }
    }
}