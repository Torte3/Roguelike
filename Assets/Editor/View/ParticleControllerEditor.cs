using UnityEngine;
using UnityEditor;
using Utilities;
using Sirenix.OdinInspector.Editor;
using View;

namespace Editor
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ParticleController))]
    public class ParticleControllerEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            ParticleController controller = (ParticleController)target;

            DrawDefaultInspector();

            SerializedObject serializedObject = new SerializedObject(controller);
            serializedObject.Update();
            SerializedProperty sortingLayerProperty = serializedObject.FindProperty("_sortingLayerID");
            SortingLayerEditorUtility.SortingLayerFieldLayout(new GUIContent("Sorting Layer"), sortingLayerProperty);

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
    }
#endif
}