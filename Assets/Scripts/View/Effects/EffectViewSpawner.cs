using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utilities;
using VContainer;

namespace View.Effects
{
    public class EffectViewSpawner
    {
        private const float PreviewAlpha = 0.25f;

        private readonly GameObject _effect;
        private readonly TextSpawner _textSpawner;

        [Inject]
        public EffectViewSpawner(TextSpawner textSpawner)
        {
            _effect = ObjectLoader.LoadPrefab("Effect");
            _textSpawner = textSpawner;
        }

        public void Spawn(IEnumerable<Vector2Int> area, Color color, int effectDisplayMilliseconds)
        {
            foreach (var position in area)
            {
                var spawnedEffect = Object.Instantiate(_effect);
                spawnedEffect.AddComponent<LifeTimer>().LifeTimeMilliseconds = effectDisplayMilliseconds;
                spawnedEffect.transform.position =
                    new Vector3(position.x, position.y, spawnedEffect.transform.position.z);
                spawnedEffect.GetComponent<SpriteRenderer>().color = color;
            }
        }

        public List<GameObject> SpawnPreview(IEnumerable<Vector2Int> area, Color color)
        {
            var spawnedEffects = new List<GameObject>();
            foreach (var position in area)
            {
                var spawnedEffect = Object.Instantiate(_effect);
                spawnedEffect.transform.position =
                    new Vector3(position.x, position.y, spawnedEffect.transform.position.z);
                spawnedEffect.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, PreviewAlpha);
                spawnedEffects.Add(spawnedEffect);
            }

            return spawnedEffects;
        }

        public List<GameObject> SpawnChargePreview(
            IEnumerable<Vector2Int> area,
            Color color,
            int turn,
            Vector2Int characterPosition)
        {
            var spawnedEffects = SpawnPreview(area, color);

            var text = _textSpawner.SpawnNumber(
                new Vector2(characterPosition.x, characterPosition.y),
                turn.ToString());
            spawnedEffects.Add(text.gameObject);

            return spawnedEffects;
        }
    }
}
