using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class RemoveUpgradeEffect : ActorlessEntityTargetEffect
    {
        [OnInspectorInit("OnProbabilityOfSuccessChanged")]
        [SerializeField]
        [Range(0, 1)]
        private float _probabilityOfSuccess = 0.1f;

        public override Color Color => Colors.SandyBrown;
        public override Impact Impact => Impact.Harmful;
#if UNITY_EDITOR
        private void OnProbabilityOfSuccessChanged()
        {
            if (_probabilityOfSuccess == 0)
                _probabilityOfSuccess = 0.1f;
        }
#endif
        public override UniTask Apply(ITargetOfEffect target, Vector2Int position, IMap map)
        {
            var upgradedItems = target.Inventory.AllItems.Where(item => item.CanDowngrade()).ToArray();
            if (upgradedItems.Any())
            {
                var item = upgradedItems.GetAtRandom();
                if (RandUtils.IsLessThanProbability(_probabilityOfSuccess))
                    item.Downgrade(target, map);
                else
                    map.Events.Record(new ItemChangeResisted(target.Entity.IsVisible, item.NameIn(map), ItemChangeKind.Downgraded));
            }
            else
            {
                map.Events.Record(new EffectMissed(target.Entity.Ref, target.Label, EffectMissReason.NoUpgradedItem));
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

        public override string Description()
        {
            return "対象の持つアイテムの強化を解除\n";
        }
    }
}