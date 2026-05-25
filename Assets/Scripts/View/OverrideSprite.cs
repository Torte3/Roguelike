using System.ComponentModel;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;

namespace View
{
    [RequireComponent(typeof(SpriteRenderer), typeof(IDirectional))]
    public class OverrideSprite : MonoBehaviour
    {
        private bool _isDirectionalTexture;
        private string _textureSubtypeName;
        private string _textureTypeName;
        private IDirectional character;
        private SpriteRenderer sr;

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            character = GetComponent<IDirectional>();
        }

        private void LateUpdate()
        {
            if (sr.enabled) sprChange();
        }

        public void SetTexture(string textureTypeName, string textureSubtypeName, bool isDirectionalTexture)
        {
            _textureTypeName = textureTypeName;
            _textureSubtypeName = textureSubtypeName;
            _isDirectionalTexture = isDirectionalTexture;
        }

        private void sprChange()
        {
            var sprName = sr.sprite.name;
            var pos = sprName.LastIndexOf('_');
            if (pos >= 0)
            {
                var sprite = GetSprite(sprName, pos);
                if (sprite != null) sr.sprite = sprite;
            }
        }

        private Sprite GetSprite(string spriteName, int index)
        {
            if (_textureSubtypeName.IsNullOrWhitespace()) return null;
            if (_isDirectionalTexture)
            {
                var direction = character.GetDirection();
                var id = int.Parse(spriteName.Substring(index + 1)) + GetIndex(direction);
                return Addressables
                    .LoadAssetAsync<Sprite>(
                        $"Assets/Images/Characters/{_textureSubtypeName}.png[{_textureSubtypeName}_{id}]")
                    .WaitForCompletion();
            }
            else
            {
                var direction = character.GetDirection();
                sr.flipX = NeedFlip(direction);
                var post = spriteName.Substring(index);
                return Addressables
                    .LoadAssetAsync<Sprite>(
                        $"Assets/Images/Monsters/{_textureSubtypeName}.png[{_textureTypeName + post}]")
                    .WaitForCompletion();
            }
        }

        public int GetIndex(Direction8 direction)
        {
            switch (direction)
            {
                case Direction8.Left: return 6;
                case Direction8.Right: return 12;
                case Direction8.Up: return 18;
                case Direction8.Down: return 0;
                case Direction8.UpLeft: return 15;
                case Direction8.DownLeft: return 3;
                case Direction8.UpRight: return 21;
                case Direction8.DownRight: return 9;
                default: return default;
            }
        }

        public bool NeedFlip(Direction8 direction)
        {
            switch (direction)
            {
                case Direction8.Left: return false;
                case Direction8.Right: return true;
                case Direction8.Up: return true;
                case Direction8.Down: return false;
                case Direction8.UpLeft: return false;
                case Direction8.DownLeft: return false;
                case Direction8.UpRight: return true;
                case Direction8.DownRight: return true;
                default: throw new InvalidEnumArgumentException();
            }
        }
    }
}