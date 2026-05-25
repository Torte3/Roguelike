using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character.Status;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Service.Logs;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class DropItemEffect : ActorlessEntityTargetEffect
    {
        [OnInspectorInit("OnProbabilityOfSuccessChanged")]
        [SerializeField]
        [Range(0, 1)]
        private float _probabilityOfSuccess = 0.5f;

        public override Color Color => Colors.MediumPurple;
        public override Impact Impact => Impact.Harmful;
#if UNITY_EDITOR
        private void OnProbabilityOfSuccessChanged()
        {
            if (_probabilityOfSuccess == 0)
                _probabilityOfSuccess = 0.5f;
        }
#endif
        public override UniTask Apply(ITargetOfEffect target, Vector2Int position, IMap map)
        {
            if (target.Status.IsFlagStat(FlagStatType.SecureHold))
            {
                GameLog.Add(target.IsVisible, $"{target.GetName(map.Player)}はアイテムを落とさなかった");
                return UniTask.CompletedTask;
            }

            var items = target.Inventory.AllItems.ToArray();
            if (items.Any())
            {
                if (RandUtils.IsLessThanProbability(_probabilityOfSuccess))
                {
                    var item = items.GetAtRandom();
                    var index = target.Inventory.GetItemIndex(item);
                    target.ForceDropItem(index.Value, map);
                }
                else
                {
                    GameLog.Add(target.IsVisible, $"{target.GetName(map.Player)}はアイテムを落とさなかった");
                }
            }
            else
            {
                GameLog.Add(target.IsVisible, $"{target.GetName(map.Player)}はアイテムを持っていない");
            }

            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.1f;
        }

        public override float EvaluatePrice()
        {
            return 50;
        }

        public override string Info()
        {
            return "対象の持つアイテムを落とさせる\n";
        }
    }
}