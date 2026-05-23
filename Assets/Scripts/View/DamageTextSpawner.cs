using TMPro;
using UnityEngine;
using VContainer;

namespace View
{
    public class DamageTextSpawner
    {
        private readonly TextSpawner _textSpawner;

        [Inject]
        public DamageTextSpawner(TextSpawner textSpawner)
        {
            _textSpawner = textSpawner;
        }

        public void ShowDamage(Vector2Int position, int value, int percentageFromMaxHP, int textDisplayMilliseconds)
        {
            SpawnText(position, value, percentageFromMaxHP, textDisplayMilliseconds, Color.red);
        }

        public void ShowHeal(Vector2Int position, int value, int percentageFromMaxHP, int textDisplayMilliseconds)
        {
            SpawnText(position, value, percentageFromMaxHP, textDisplayMilliseconds, Color.green);
        }

        private void SpawnText(Vector2Int position, int value, int percentageFromMaxHP, int textDisplayMilliseconds,
            Color color)
        {
            var text = _textSpawner.SpawnNumber(position + new Vector2(0, 0.5f), value.ToString());
            ApplyFontSize(text, percentageFromMaxHP);
            text.color = color;
            text.gameObject.AddComponent<DamageText>();
            text.gameObject.AddComponent<LifeTimer>().LifeTimeMilliseconds = textDisplayMilliseconds;
        }

        private static void ApplyFontSize(TextMeshProUGUI text, int percentageFromMaxHP)
        {
            if (percentageFromMaxHP > 100)
            {
                text.fontSize = 1f;
            }
            else if (percentageFromMaxHP > 25)
            {
                text.fontSize = 0.8f;
            }
            else if (percentageFromMaxHP > 10)
            {
                text.fontSize = 0.7f;
            }
            else
            {
                text.fontSize = 0.5f;
            }
        }

        public void DeleteAllText()
        {
            _textSpawner.DeleteAll<DamageText>();
        }
    }
}
