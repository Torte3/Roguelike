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

            TryRefreshPrices();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            DrawArtifactInfoPreview();
            EditorGUILayout.Space();

            if (EditorGUI.EndChangeCheck())
                TryRefreshPrices();

            EditorGUILayout.LabelField($"Evaluated Price: {_evaluatedPrice}G");
            EditorGUILayout.LabelField($"Market Price: {_marketPrice}G");
        }

        private void TryRefreshPrices()
        {
            ItemInspectorPreviewEditor.DrawSafe(() =>
            {
                _evaluatedPrice = EvaluateEvaluatedPrice();
                _marketPrice = EvaluateMarketPrice();
            }, "価格の算出に失敗しました。");
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
            ItemInspectorPreviewEditor.DrawSafe(() =>
            {
                var item = new EquipmentItem((ArtifactData)target);
                ItemDescriptionPreviewEditor.DrawIdentifiedLikeInventory(item.FullInfoGenericSkillDescription(), 120f);
            });
        }
    }
}
