using Domain.Model.Item;
using Domain.Service.Items;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Editor
{
    [CustomEditor(typeof(DirectWeaponData))]
    public class DirectWeaponDataEditor : OdinEditor
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

            DrawDescriptionTemplateSection();
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
            DirectWeaponData weaponData = (DirectWeaponData)target;
            DirectWeapon item = new DirectWeapon(weaponData);
            return item.EvaluateEvaluatedPrice();
        }

        private float EvaluateMarketPrice()
        {
            DirectWeaponData weaponData = (DirectWeaponData)target;
            DirectWeapon item = new DirectWeapon(weaponData);
            return item.EvaluatePrice(_cachedMarketPriceTable!);
        }

        private void DrawDescriptionTemplateSection()
        {
            var data = (DirectWeaponData)target;
            EditorGUILayout.LabelField("説明文テンプレート（近接武器）", EditorStyles.boldLabel);
            var errors = ItemDescriptionTemplate.ValidateDirectWeapon(data);
            foreach (var message in errors)
                EditorGUILayout.HelpBox(message, MessageType.Error);
            if (errors.Count == 0)
                EditorGUILayout.HelpBox("テンプレート整合性: 問題なし", MessageType.Info);

            EditorGUILayout.LabelField("ゲーム内プレビュー（識別済み・効果要約・色付き）");
            ItemInspectorPreviewEditor.DrawSafe(() =>
            {
                var preview = new DirectWeapon(data).PreviewTemplatedSkillSection();
                ItemDescriptionPreviewEditor.DrawIdentifiedLikeInventory(
                    string.IsNullOrEmpty(preview) ? "（要約なし）" : preview,
                    80f);
            });

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("識別済み・汎用説明（効果はテンプレート不使用・色付き）");
            ItemInspectorPreviewEditor.DrawSafe(() =>
            {
                var generic = new DirectWeapon(data).FullInfoGenericSkillDescription();
                ItemDescriptionPreviewEditor.DrawIdentifiedLikeInventory(generic, 120f);
            });
        }
    }
}
