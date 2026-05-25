using Domain.Model.Character;
using Domain.Model.Character.Type;
using Domain.Service.Characters;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Editor
{
    [CustomEditor(typeof(EnemyData))]
    public class EnemyDataEditor : OdinEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateSprite();
        }
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateSprite();
            }
        }

        private void UpdateSprite()
        {
            var characterType = ((EnemyData)target).CharacterType;
            if (characterType is Human human)
            {
                var sprite = Addressables
                    .LoadAssetAsync<Sprite>(
                        $"Assets/Images/Characters/{human.TextureName}.png[{human.TextureName}_0]")
                    .WaitForCompletion();
                ((EnemyData)target)._sprite = sprite;
            }
            else
            {
                var sprite = Addressables
                    .LoadAssetAsync<Sprite>(
                        $"Assets/Images/Monsters/{characterType.SubtypeName()}.png")
                    .WaitForCompletion();
                ((EnemyData)target)._sprite = sprite;
            }
        }
    }
}
