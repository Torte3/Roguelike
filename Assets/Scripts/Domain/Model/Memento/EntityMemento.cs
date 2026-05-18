#nullable enable
using System;
using Domain.Model.Entity;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class EntityMemento
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public Vector2Int Position { get; private set; }
        [field: SerializeField] public EntityLayer Layer { get; private set; }
        [field: SerializeField] public bool IgnoreGrass { get; private set; }
        [field: SerializeField] public Option<string> DestroyLog { get; private set; }

        public EntityMemento(
            string id,
            Vector2Int position,
            EntityLayer layer,
            Option<string> destroyLog,
            bool ignoreGrass = false)
        {
            Id = id;
            Position = position;
            Layer = layer;
            IgnoreGrass = ignoreGrass;
            DestroyLog = destroyLog;
        }

        public EntityMemento CopyWith(
            string? id = null,
            Vector2Int? position = null,
            EntityLayer? layer = null,
            Option<string>? destroyLog = null,
            bool? ignoreGrass = null)
        {
            return new EntityMemento(
                id ?? Id,
                position ?? Position,
                layer ?? Layer,
                destroyLog ?? DestroyLog,
                ignoreGrass ?? IgnoreGrass
            );
        }
    }
}