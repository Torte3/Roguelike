#nullable enable

using System;
using System.Collections.Generic;
using Domain.Model.Condition;
using UnityEngine;

namespace Domain.Model.Item
{
    [Serializable]
    public class ArtifactPassiveConditionBundle
    {
        [SerializeField] private string _displayName = "";

        [SerializeReference]
        private List<IConditionData> _conditions = new();

        public string DisplayName => _displayName;

        public IReadOnlyList<IConditionData> Conditions => _conditions;

        public ArtifactPassiveConditionBundle()
        {
            _displayName = "";
            _conditions = new List<IConditionData>();
        }

        public ArtifactPassiveConditionBundle(string displayName, List<IConditionData> conditions)
        {
            _displayName = displayName;
            _conditions = conditions;
        }

        public ArtifactPassiveConditionBundle Clone()
        {
            var wrapper = new JsonCloneWrapper { Bundle = this };
            var json = JsonUtility.ToJson(wrapper);
            var decoded = JsonUtility.FromJson<JsonCloneWrapper>(json);
            return decoded.Bundle;
        }

        [Serializable]
        private class JsonCloneWrapper
        {
            public ArtifactPassiveConditionBundle Bundle = null!;
        }
    }
}
