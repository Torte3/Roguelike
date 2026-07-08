#nullable enable
using Domain.Model.Character.Affiliation;
using Domain.Model.Character.Behavior;
using Domain.Model.Character.Status;
using Domain.Model.Character.Type;
using Domain.Model.Effect.Elements;
using Domain.Model.Effect.Skill;
using Domain.Model.Evaluation;
using Domain.Model.Item.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Table;
using Utilities.Serialize;
using System.Collections.Generic;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Domain.Model.Character.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [ReadOnly][Required] public string Name = "";
        public CharacterGroup Group = CharacterGroup.Monster;
        [SerializeReference][Required] public ICharacterType CharacterType;
        [PreviewField(Alignment = ObjectFieldAlignment.Left), ReadOnly] public Sprite _sprite;
        public bool IsBoss;
        [MinValue(1)] public int Hp;
        public Aggression Aggression = Aggression.AvoidAllies;
        public BehaviorData Behavior;
        public MoveSpeed MoveSpeed = MoveSpeed.Normal;
        public List<FlagStatType> Flags;
        public bool IsFlying;
        public bool CanThroughWalls;
        public bool CanMimic;
        [ShowIf("@" + nameof(CanMimic))] public MimicWeights MimicWeights = new();
        public bool CanPickUp;
        public bool CanUseItem;
        public bool CanReceivePlayerGift;
        public List<CharacterSkillWithRuleData> Skills;
        public bool HasLastSkill;
        [ShowIf("@" + nameof(HasLastSkill))] public SkillData LastSkill;
        [MinValue(0)] public float AttackMultiplier = 1f;
        public SerializableDictionary<Element, float> ElementAttackMultiplier;
        public SerializableDictionary<Element, float> ElementDamageRateMultiplier;
        public SerializableDictionary<ConditionTemplate, float> ConditionResistance;
        [Range(0, 1)] public float DropItemRate;
        [ShowIf("@" + nameof(DropItemRate) + " > 0")] public Table<ItemData> DropItemTable;
#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            // assetPath は Play 中や Addressables ロード時などに空を返すことがある。
            // その際に Name を空文字で上書きしてしまう（敵名が消える）ため、空ならガードする。
            var newName = !string.IsNullOrEmpty(fileName) ? fileName : Name;
            var newFlags = Flags.Distinct().ToList();
            var newCanReceivePlayerGift = newName != "店員" && CanUseItem;

            // 値が変わっていないのに SetDirty すると、インポート順の都合で未解決の _sprite 参照が
            // 再シリアライズで {fileID: 0} に消える（プレビュー消失・PPtr 破損エラー）。
            // 実際に変化があるときだけ Dirty にして、不要な書き換えを防ぐ。
            var changed = Name != newName
                          || CanReceivePlayerGift != newCanReceivePlayerGift
                          || !Flags.SequenceEqual(newFlags);

            Name = newName;
            Flags = newFlags;
            CanReceivePlayerGift = newCanReceivePlayerGift;

            foreach (var skill in Skills)
            {
                skill.Skill.OnValidate(CommonSenseParameters.SkillOnUseProbabilityOfSuccess);
            }

            if (LastSkill != null)
            {
                LastSkill.OnValidate(1);
            }

            if (changed)
                EditorUtility.SetDirty(this);
        }
#endif
    }
    [Serializable]
    public class CharacterSkillWithRuleData
    {
        [Required] public SkillData Skill;
        [MinValue(0)] public int Priority;
    }
}