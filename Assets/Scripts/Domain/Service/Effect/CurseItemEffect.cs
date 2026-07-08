using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character.Status;
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
                map.Events.Record(new EffectMissed(target.Entity.Ref, target.Label, EffectMissReason.CannotBeCursed));
                return UniTask.CompletedTask;
            }

            var notCursedItems = target.Inventory.AllItems.Where(item => !item.IsCursed).ToArray();
            if (notCursedItems.Any())
            {
                var item = notCursedItems.GetAtRandom();
                if (RandUtils.IsLessThanProbability(_probabilityOfSuccess))
                    item.SetCursed(target, map, true);
                else
                    map.Events.Record(new ItemChangeResisted(target.Entity.IsVisible, item.NameIn(map), ItemChangeKind.Cursed));
            }
            else
            {
                map.Events.Record(new EffectMissed(target.Entity.Ref, target.Label, EffectMissReason.NoItemToCurse));
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
            return "対象の持つアイテムに呪い付与\n";
        }
    }
}