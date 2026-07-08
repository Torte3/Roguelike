using UnityEngine;
using VContainer;

namespace View.Effects
{
    public class DamageTextSpawner
    {
        private readonly TextSpawner _textSpawner;

        [Inject]
        public DamageTextSpawner(TextSpawner textSpawner)
        {
            _textSpawner = textSpawner;
        }

        public void ShowDamage(Vector2Int position, int value, float fontSize, int textDisplayMilliseconds)
        {
            SpawnText(position, value, fontSize, textDisplayMilliseconds, Color.red);
        }

        public void ShowHeal(Vector2Int position, int value, float fontSize, int textDisplayMilliseconds)
        {
            SpawnText(position, value, fontSize, textDisplayMilliseconds, Color.green);
        }

        private void SpawnText(Vector2Int position, int value, float fontSize, int textDisplayMilliseconds,
            Color color)
        {
            var text = _textSpawner.SpawnNumber(position + new Vector2(0, 0.5f), value.ToString());
            text.fontSize = fontSize;
            text.color = color;
            text.gameObject.AddComponent<DamageText>();
            text.gameObject.AddComponent<LifeTimer>().LifeTimeMilliseconds = textDisplayMilliseconds;
        }

        public void DeleteAllText()
        {
            _textSpawner.DeleteAll<DamageText>();
        }
    }
}
