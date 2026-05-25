using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class ItemDescriptionPreviewEditor
    {
        private static GUIStyle _richWrappedStyle;

        private static GUIStyle RichWrappedStyle
        {
            get
            {
                if (_richWrappedStyle == null)
                {
                    _richWrappedStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
                    {
                        richText = true,
                    };
                }
                return _richWrappedStyle;
            }
        }

        public static void DrawIdentifiedLikeInventory(string description, float minHeight = 80f) =>
            DrawRichText(description, minHeight);

        public static void DrawRichText(string richText, float minHeight = 80f)
        {
            if (string.IsNullOrEmpty(richText))
                return;
            var style = RichWrappedStyle;
            var width = Mathf.Max(EditorGUIUtility.currentViewWidth - 28f, 50f);
            var h = Mathf.Max(style.CalcHeight(new GUIContent(richText), width), minHeight);
            var rect = EditorGUILayout.GetControlRect(false, h);
            EditorGUI.SelectableLabel(rect, richText, style);
        }
    }
}
