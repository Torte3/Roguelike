using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal static class ItemInspectorPreviewEditor
    {
        public static void DrawSafe(Action draw, string failureMessage = "プレビュー生成に失敗しました。")
        {
            try
            {
                draw();
            }
            catch (Exception ex)
            {
                EditorGUILayout.HelpBox(
                    failureMessage + "\n" + ex.Message,
                    MessageType.Warning);
            }
        }
    }
}
