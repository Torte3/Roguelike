#nullable enable
using Domain.Model.Character.Status;
using Domain.Model.Character.Type;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Serialize;
using System.Collections.Generic;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Domain.Model.Character
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [ReadOnly][Required] public string Name = "";
        [SerializeField] public Human CharacterType;
        [PreviewField(Alignment = ObjectFieldAlignment.Left), ReadOnly] public Sprite _sprite;
        public bool IsBoss;
        [MinValue(1)] public int Hp;
        public MoveSpeed MoveSpeed = MoveSpeed.Normal;
        [MinValue(0)] public int InventoryCapacity = 20;
        public List<FlagStatType> Flags;
        public bool IsFlying;
        public bool CanThroughWalls;
        [MinValue(0)] public float AttackMultiplier = 1f;
        public SerializableDictionary<Element, float> ElementAttackMultiplier;
        public SerializableDictionary<Element, float> ElementDamageRateMultiplier;
        public SerializableDictionary<ConditionTemplate, float> ConditionResistance;
        public string DescriptionWithoutName()
        {
            var description = "";
            description += $"HP: {Hp}\n";
            description += $"所持上限: {InventoryCapacity}\n";
            if (MoveSpeed != MoveSpeed.Normal)
                description += $"速度: {MoveSpeed.GetName()}\n";
            foreach (var flag in Flags)
            {
                description += $"{flag.GetName()}\n";
            }
            if (IsFlying)
                description += $"飛行\n";
            if (CanThroughWalls)
                description += $"壁を貫通可能\n";
            description += $"\n";
            if (AttackMultiplier != 1f)
                description += $"攻撃倍率: {AttackMultiplier:P0}\n";
            foreach (var element in ElementAttackMultiplier.Keys)
            {
                description += $"{element.Name()}属性攻撃倍率: {ElementAttackMultiplier[element]:P0}\n";
            }
            foreach (var element in ElementDamageRateMultiplier.Keys)
            {
                description += $"{element.Name()}属性被ダメージ倍率: {ElementDamageRateMultiplier[element]:P0}\n";
            }
            foreach (var condition in ConditionResistance.Keys)
            {
                description += $"{condition.name}耐性: {ConditionResistance[condition]:P0}\n";
            }
            return description;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            Name = Path.GetFileNameWithoutExtension(assetPath);

            Flags = Flags.Distinct().ToList();

            EditorUtility.SetDirty(this);
        }
#endif
    }
}