#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    public class EquipToggleSkill : ISerializable<EquipToggleSkillMemento>, ISkill
    {
        public bool IsDirectional => false;

        public EquipToggleSkill(EquipToggleSkillMemento _)
        {
        }

        public EquipToggleSkillMemento Serialize() => new();

        public static EquipToggleSkillMemento BuildMemento() => new();

        public UniTask<ISkillResult> Use(IActorOfEffect actor, IItem item, Vector2Int position, Direction8 direction, IMap map)
        {
            if (item is IEquipmentToggleTarget toggleTarget)
            {
                if (!toggleTarget.TryToggleEquipped(actor, map))
                    return UniTask.FromResult((ISkillResult)SpawnEffectSkillResult.Failed);
                return UniTask.FromResult((ISkillResult)SpawnEffectSkillResult.Success);
            }

            throw new ArgumentException($"Expected {nameof(IEquipmentToggleTarget)}, got {item.GetType().Name}");
        }

        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map) => 0.01f;

        public float EvaluatePrice() => 0;

        public string Info() => "装備する / 外す\n";
    }
}
