using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Service.Items;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Editor
{
    [CustomEditor(typeof(ItemData))]
    public class ItemDataEditor : OdinEditor
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
            var itemData = (ItemData)target;

            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            if (itemData.Category == ItemCategory.Potions)
            {
                DrawPotionDescriptionTemplateSection(itemData);
                EditorGUILayout.Space();
            }

            DrawGenericItemDescriptionPreview(itemData);
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
            ItemData itemData = (ItemData)target;
            Item item = new Item(itemData);
            return item.EvaluateEvaluatedPrice();
        }

        private float EvaluateMarketPrice()
        {
            ItemData itemData = (ItemData)target;
            Item item = new Item(itemData);
            return item.EvaluatePrice(_cachedMarketPriceTable!);
        }

        private static void DrawPotionDescriptionTemplateSection(ItemData itemData)
        {
            EditorGUILayout.LabelField("説明文テンプレート（ポーション）", EditorStyles.boldLabel);
            var errors = ItemDescriptionTemplate.ValidatePotionItemData(itemData);
            foreach (var message in errors)
                EditorGUILayout.HelpBox(message, MessageType.Error);
            if (errors.Count == 0)
                EditorGUILayout.HelpBox("テンプレート整合性: 問題なし", MessageType.Info);

            EditorGUILayout.LabelField("ゲーム内プレビュー（識別済み・効果要約・色付き）");
            ItemInspectorPreviewEditor.DrawSafe(() =>
            {
                var preview = new Item(itemData).PreviewTemplatedSkillSection();
                ItemDescriptionPreviewEditor.DrawIdentifiedLikeInventory(
                    string.IsNullOrEmpty(preview) ? "（要約なし・詳細表示にフォールバック）" : preview,
                    80f);
            });
        }

        private static void DrawGenericItemDescriptionPreview(ItemData itemData)
        {
            EditorGUILayout.LabelField("ゲーム内プレビュー（識別済み・汎用説明・効果はテンプレート不使用・色付き）", EditorStyles.boldLabel);
            ItemInspectorPreviewEditor.DrawSafe(() =>
            {
                var item = new Item(itemData);
                ItemDescriptionPreviewEditor.DrawIdentifiedLikeInventory(item.FullInfoGenericSkillDescription(), 120f);
            });
        }
    }
}
