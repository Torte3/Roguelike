#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Domain.Model.Condition;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using UnityEditor;
using UnityEngine;

namespace Editor.Migration
{
    /// <summary>
    /// <see cref="ItemData"/>（Category 指輪）を <see cref="ArtifactData"/> に置き換える。
    /// PassiveConditions はすべて1つの <see cref="ArtifactPassiveConditionBundle"/> にまとめる。
    /// </summary>
    public static class MigrateRingItemDataToArtifactData
    {
        private const string MenuPath = "Tools/Migration/指輪 ItemData → ArtifactData";

        [MenuItem(MenuPath)]
        private static void Run()
        {
            if (!EditorUtility.DisplayDialog(
                    "指輪データの移行",
                    "Assets/Database/ItemData/指輪 以下の ItemData（Artifacts）を ArtifactData に置き換えます。\n" +
                    "・SynthesisSlotLimit = 1\n" +
                    "・DisplayName = {接頭辞}の指輪の{接頭辞}（アセット名が「○○の指輪」のとき）\n" +
                    "・PassiveConditions は1バンドルに統合\n\n" +
                    "バックアップ推奨。続行しますか？",
                    "実行",
                    "キャンセル"))
                return;

            var itemDataGuids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Database/ItemData/指輪" });
            var migrated = 0;
            var skipped = 0;

            try
            {
                for (var i = 0; i < itemDataGuids.Length; i++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(itemDataGuids[i]);
                    EditorUtility.DisplayProgressBar(
                        "指輪の移行",
                        path,
                        itemDataGuids.Length > 0 ? (float)i / itemDataGuids.Length : 1f);

                    var itemData = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                    if (itemData == null || itemData.Category != ItemCategory.Artifacts)
                    {
                        skipped++;
                        continue;
                    }

                    var oldGuid = itemDataGuids[i];
                    var tempPath = path.Replace(".asset", ".MigrationTemp.asset");
                    var oldMetaPath = path + ".meta";

                    if (File.Exists(tempPath))
                        AssetDatabase.DeleteAsset(tempPath);

                    var artifact = BuildArtifactData(itemData);
                    AssetDatabase.CreateAsset(artifact, tempPath);
                    var newGuid = AssetDatabase.AssetPathToGUID(tempPath);
                    if (string.IsNullOrEmpty(newGuid))
                    {
                        Debug.LogError($"GUID を取得できません: {tempPath}");
                        AssetDatabase.DeleteAsset(tempPath);
                        skipped++;
                        continue;
                    }

                    ReplaceGuidInTextAssets(oldGuid, newGuid, oldMetaPath, tempPath);

                    AssetDatabase.DeleteAsset(path);
                    var moveError = AssetDatabase.MoveAsset(tempPath, path);
                    if (!string.IsNullOrEmpty(moveError))
                    {
                        Debug.LogError($"MoveAsset 失敗: {moveError} ({tempPath} → {path})");
                        skipped++;
                        continue;
                    }

                    migrated++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("指輪の移行", $"完了: {migrated} 件置換、スキップ: {skipped} 件", "OK");
        }

        private static ArtifactData BuildArtifactData(ItemData source)
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactData>();
            artifact.name = source.name;
            artifact.Icon = source.Icon;
            artifact.IsShiny = source.IsShiny;
            artifact.UseCustomBasePrice = source.UseCustomBasePrice;
            artifact.CustomBasePrice = source.CustomBasePrice;
            artifact.AdditionalPrice = source.AdditionalPrice;
            artifact.MultiplyPrice = source.MultiplyPrice;
            artifact.SynthesisSlotLimit = 1;

            var soSource = new SerializedObject(source);
            var soArtifact = new SerializedObject(artifact);
            soArtifact.FindProperty("_rarity").enumValueIndex = soSource.FindProperty("_rarity").enumValueIndex;
            soArtifact.ApplyModifiedPropertiesWithoutUndo();

            var passives = source.PassiveConditions;
            if (passives != null && passives.Count > 0)
            {
                artifact.HasBuiltInPassive = true;
                var displayName = MakeDisplayName(source.name);
                var tempBundle = new ArtifactPassiveConditionBundle(displayName, new List<IConditionData>(passives));
                artifact.BuiltInPassiveConditionBundle = tempBundle.Clone();
            }
            else
                artifact.HasBuiltInPassive = false;

            return artifact;
        }

        /// <summary>「○○の指輪」→「○○の指輪の○○」。それ以外は「{名}の指輪の{名}」。</summary>
        private static string MakeDisplayName(string assetName)
        {
            const string suffix = "の指輪";
            if (assetName.EndsWith(suffix))
            {
                var prefix = assetName.Substring(0, assetName.Length - suffix.Length);
                return $"{prefix}の指輪の{prefix}";
            }

            return $"{assetName}の指輪の{assetName}";
        }

        /// <summary>移行元 .meta を書き換えると GUID が衝突するため除外する。</summary>
        private static void ReplaceGuidInTextAssets(string oldGuid, string newGuid, string excludeMetaPath, string tempAssetPath)
        {
            if (oldGuid == newGuid)
                return;

            var tempMetaPath = tempAssetPath + ".meta";
            var paths = AssetDatabase.GetAllAssetPaths();
            foreach (var assetPath in paths)
            {
                if (!assetPath.StartsWith("Assets/"))
                    continue;
                // GetAllAssetPaths はフォルダも含む。ディレクトリを ReadAllText すると拒否例外になる。
                if (AssetDatabase.IsValidFolder(assetPath))
                    continue;
                if (assetPath.EndsWith(".meta") && assetPath == excludeMetaPath)
                    continue;
                if (assetPath == tempAssetPath || assetPath == tempMetaPath)
                    continue;
                if (assetPath.EndsWith(".cs") || assetPath.EndsWith(".dll") ||
                    assetPath.EndsWith(".png") || assetPath.EndsWith(".jpg") || assetPath.EndsWith(".wav"))
                    continue;

                try
                {
                    var text = File.ReadAllText(assetPath);
                    if (!text.Contains(oldGuid))
                        continue;
                    File.WriteAllText(assetPath, text.Replace(oldGuid, newGuid));
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.Default);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    // バイナリ・ロック・フォルダ誤検知など
                }
            }
        }
    }
}
#endif
