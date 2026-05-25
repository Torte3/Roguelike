using UnityEngine;
using Utilities;

namespace View
{
    [RequireComponent(typeof(SpriteRenderer), typeof(OverrideSprite))]
    public class CharacterDemoDisplay : MonoBehaviour, IDirectional
    {
        private OverrideSprite _overrideSprite;
        private string _textureName;

        private void Awake()
        {
            _overrideSprite = GetComponent<OverrideSprite>();
        }

        public void SetTexture(string textureName)
        {
            _textureName = textureName;
            _overrideSprite.SetTexture("Human", _textureName, true);
        }

        public void SetColor(Color color)
        {
            GetComponent<SpriteRenderer>().color = color;
        }

        public Direction8 GetDirection()
        {
            return Direction8.Down;
        }
    }
}