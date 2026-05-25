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
    public class CurseItemEffect : ActorlessEntityTargetEffect
    {
        [OnInspectorInit("OnProbabilityOfSuccessChanged")]
        [SerializeField]
        [Range(0, 1)]
        private float _probabilityOfSuccess = 0.25f;

        public override Color Color => Colors.MediumPurple;
        public override Impact Impact => Impact.Harmful;
#if UNITY_EDITOR
        private void OnProbabilityOfSuccessChanged()
        {
            if (_probabilityOfSuccess == 0)
                _probabilityOfSuccess = 0.25f;
        }
#endif
        public override UniTask Apply(ITargetOfEffect target, Vector2Int position, IMap map)
        {
            if (target.Status.IsFlagStat(FlagStatType.CurseProof))
            {
                GameLog.Add(target.IsVisible, $"{target.GetName(map.Player)}は呪われない");
                return UniTask.CompletedTask;
            }

            var notCursedItems = target.Inventory.AllItems.Where(item => !item.IsCursed).ToArray();
            if (notCursedItems.Any())
            {
                var item = notCursedItems.GetAtRandom();
                if (RandUtils.IsLessThanProbability(_probabilityOfSuccess))
                    item.SetCursed(map.Player, target, map.ItemPlaceholders, true);
                else
                    GameLog.Add(target.IsVisible, $"{item.GetName(map.Player, map.ItemPlaceholders)}は呪われなかった");
            }
            else
            {
                GameLog.Add(target.IsVisible, $"{target.GetName(map.Player)}は呪いの対象になるアイテムを持っていない");
            }

            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.25f;
        }

        public override float EvaluatePrice()
        {
            return 100;
        }

        public override string Info()
        {
            return "対象の持つアイテムに呪い付与\n";
        }
    }
}