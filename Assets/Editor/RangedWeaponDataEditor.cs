using System;
using Domain.Model.Item;
using Domain.Service.Items;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Editor
{
    [CustomEditor(typeof(RangedWeaponData))]
    public class RangedWeaponDataEditor : OdinEditor
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
            DrawDescriptionTemplateSection();
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
            var weaponData = (RangedWeaponData)target;
            var item = new RangedWeapon(weaponData);
            return item.EvaluateEvaluatedPrice();
        }

        private float EvaluateMarketPrice()
        {
            var weaponData = (RangedWeaponData)target;
            var item = new RangedWeapon(weaponData);
            return item.EvaluatePrice(_cachedMarketPriceTable!);
        }

        private void DrawDescriptionTemplateSection()
        {
            var data = (RangedWeaponData)target;
            EditorGUILayout.LabelField("説明文テンプレート（射撃武器）", EditorStyles.boldLabel);
            var errors = ItemDescriptionTemplate.ValidateRangedWeapon(data);
            foreach (var message in errors)
                EditorGUILayout.HelpBox(message, MessageType.Error);
            if (errors.Count == 0)
                EditorGUILayout.HelpBox("テンプレート整合性: 問題なし", MessageType.Info);

            EditorGUILayout.LabelField("ゲーム内プレビュー（識別済み・効果要約・色付き）");
            var preview = new RangedWeapon(data).PreviewTemplatedSkillSection();
            ItemDescriptionPreviewEditor.DrawIdentifiedLikeInventory(
                string.IsNullOrEmpty(preview) ? "（要約なし）" : preview,
                80f);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("識別済み・汎用説明（効果はテンプレート不使用・色付き）");
            try
            {
                var generic = new RangedWeapon(data).FullInfoGenericSkillDescription();
                ItemDescriptionPreviewEditor.DrawIdentifiedLikeInventory(generic, 120f);
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
