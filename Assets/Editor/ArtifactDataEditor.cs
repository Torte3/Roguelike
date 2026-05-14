using System;
using Domain.Model.Item;
using Domain.Service.Items;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Editor
{
    [CustomEditor(typeof(ArtifactData))]
    public class ArtifactDataEditor : OdinEditor
    {
        private float _evaluatedPrice;
        private float _marketPrice;
        private static ItemMarketPriceTable? _cachedMarketPriceTable;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_cachedMarketPriceTable == null)
            {
                _cachedMarketPriceTable = Addressables.LoadAssetAsync<ItemMarketPriceTable>("Assets/Database/ItemData/ItemMarketPriceTable.asset")
                    .WaitForCompletion();
            }

            _evaluatedPrice = EvaluateEvaluatedPrice();
            _marketPrice = EvaluateMarketPrice();
        }

        public override void OnInspectorGUI()
        {
            DrawArtifactInfoPreview();
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            if (EditorGUI.EndChangeCheck())
            {
                _evaluatedPrice = EvaluateEvaluatedPrice();
                _marketPrice = EvaluateMarketPrice();
            }

            EditorGUILayout.LabelField($"Evaluated Price: {_evaluatedPrice}G");
            EditorGUILayout.LabelField($"Market Price: {_marketPrice}G");
        }

        private float EvaluateEvaluatedPrice()
        {
            var data = (ArtifactData)target;
            var item = new EquipmentItem(data);
            return item.EvaluateEvaluatedPrice();
        }

        private float EvaluateMarketPrice()
        {
            var data = (ArtifactData)target;
            var item = new EquipmentItem(data);
            return item.EvaluatePrice(_cachedMarketPriceTable!);
        }

        private void DrawArtifactInfoPreview()
        {
            EditorGUILayout.LabelField("ゲーム内プレビュー（識別済み・汎用説明・色付き）", EditorStyles.boldLabel);
            try
            {
                var item = new EquipmentItem((ArtifactData)target);
                ItemDescriptionPreviewEditor.DrawIdentifiedLikeInventory(item.FullInfoGenericSkillDescription(), 120f);
            }
            catch (Exception ex)
            {
                EditorGUILayout.HelpBox(
                    "プレビュー生成に失敗しました。参照先アセット未設定の可能性があります。\n" + ex.Message,
                    MessageType.Warning);
            }
        }
    }
}
