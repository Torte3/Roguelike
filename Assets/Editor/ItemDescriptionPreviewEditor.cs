using Domain.Service.Items;
using Provider;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>インスペクタでアイテム説明を TMP と同様の &lt;color&gt; 付きで表示する。</summary>
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

        /// <summary>ゲーム内と同じ説明文（組み立て時に色タグ済み）を表示。</summary>
        public static void DrawIdentifiedLikeInventory(string description, float minHeight = 80f) =>
            DrawRichText(description, minHeight);

        public static string GenericDescriptionOf(BaseItem item) =>
            ItemDescriptionText.Of(item.DescribeIdentifiedWithoutSkillTemplate());

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
