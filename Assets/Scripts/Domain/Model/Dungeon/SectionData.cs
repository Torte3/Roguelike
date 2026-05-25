using Domain.Model.Character;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Dungeon
{
    [CreateAssetMenu(fileName = "Section", menuName = "ScriptableObject/Section")]
    public class SectionData : ScriptableObject
    {
        public MapType Type;
        public bool RoundRoomCorner;
        public bool CaveInOneRoom;
        [Range(0, 1)] public float WaterChance;
        [Range(0, 1)] public float GrassChance = 0.3f;
        [Range(0, 1)] public float LakeChance = 0.1f;
        [MinValue(0)] public float TrapCount = 0.5f;
        [Required] public EnemyData Mimic;
        [Required] public EnemyData Clerk;
    }
}
