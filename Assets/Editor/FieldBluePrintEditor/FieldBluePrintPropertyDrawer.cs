using RandomDungeonWithBluePrint;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(FieldBluePrint))]
    public class FieldBluePrintPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // プロパティの描画領域を計算
            Rect propertyRect = new Rect(position.x, position.y, position.width - 60, position.height);
            Rect buttonRect = new Rect(position.x + position.width - 55, position.y, 50, position.height);

            // FieldBluePrintオブジェクトフィールドを描画
            EditorGUI.PropertyField(propertyRect, property, label, true);

            // Editボタンを描画
            if (property.objectReferenceValue != null)
            {
                if (GUI.Button(buttonRect, "Edit"))
                {
                    var blueprint = property.objectReferenceValue as FieldBluePrint;
                    if (blueprint != null)
                    {
                        var window = EditorWindow.GetWindow<FieldBluePrintEditor>("Field Blue Print Editor");
                        window.LoadBlueprint(blueprint);
                    }
                }
            }
            else
            {
                GUI.enabled = false;
                GUI.Button(buttonRect, "Edit");
                GUI.enabled = true;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
