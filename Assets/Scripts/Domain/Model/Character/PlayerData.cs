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
        public string InfoWithoutName()
        {
            var info = "";
            info += $"HP: {Hp}\n";
            info += $"所持上限: {InventoryCapacity}\n";
            if (MoveSpeed != MoveSpeed.Normal)
                info += $"速度: {MoveSpeed.GetName()}\n";
            foreach (var flag in Flags)
            {
                info += $"{flag.GetName()}\n";
            }
            if (IsFlying)
                info += $"飛行\n";
            if (CanThroughWalls)
                info += $"壁を貫通可能\n";
            info += $"\n";
            if (AttackMultiplier != 1f)
                info += $"攻撃倍率: {AttackMultiplier:P0}\n";
            foreach (var element in ElementAttackMultiplier.Keys)
            {
                info += $"{element.Name()}属性攻撃倍率: {ElementAttackMultiplier[element]:P0}\n";
            }
            foreach (var element in ElementDamageRateMultiplier.Keys)
            {
                info += $"{element.Name()}属性被ダメージ倍率: {ElementDamageRateMultiplier[element]:P0}\n";
            }
            foreach (var condition in ConditionResistance.Keys)
            {
                info += $"{condition.name}耐性: {ConditionResistance[condition]:P0}\n";
            }
            return info;
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