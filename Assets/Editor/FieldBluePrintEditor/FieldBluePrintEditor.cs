using RandomDungeonWithBluePrint;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(FieldBluePrint))]
    public class FieldBluePrintInspectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // デフォルトのInspectorを描画
            DrawDefaultInspector();

            EditorGUILayout.Space();

            // Editボタンを追加
            if (GUILayout.Button("Open in Field Blue Print Editor", GUILayout.Height(30)))
            {
                var blueprint = target as FieldBluePrint;
                if (blueprint != null)
                {
                    var window = EditorWindow.GetWindow<FieldBluePrintEditor>("Field Blue Print Editor");
                    window.LoadBlueprint(blueprint);
                }
            }
        }
    }
}
