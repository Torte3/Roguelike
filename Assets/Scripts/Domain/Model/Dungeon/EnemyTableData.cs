using Domain.Model.Character;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Table;

namespace Domain.Model.Dungeon
{
    [CreateAssetMenu(fileName = "EnemyTable", menuName = "ScriptableObject/Enemy Table")]
    public class EnemyTableData : ScriptableObject
    {
        [RequiredListLength(1, null)]
        public Table<EnemyData> Enemies = new();
    }
}
