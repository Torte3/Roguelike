#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class FloorSpecMemento
    {
        [field: SerializeField] public string SectionDataName { get; private set; }
        [field: SerializeField] public string FieldBluePrintName { get; private set; }
        [field: SerializeField] public List<string> EnemyNames { get; private set; }

        public FloorSpecMemento(string sectionDataName, string fieldBluePrintName, List<string> enemyNames)
        {
            SectionDataName = sectionDataName;
            FieldBluePrintName = fieldBluePrintName;
            EnemyNames = enemyNames;
        }
    }
}
